using MultiConverter.Lib.Interface;
using MultiConverter.Lib.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiConverter.Lib.Format
{
    public class SKIN : IWowFile
    {
        private HashSet<uint> BadSubmesh;

        public SKIN(string skin) : base(skin) => BadSubmesh = new HashSet<uint>();

        public void Fix(ref List<short> textureUnitLookup, ref List<ushort> blendmodeOverride, int nTransparencyLookup)
        {
            FixSubmesh();
            FixTexUnit(ref textureUnitLookup, ref blendmodeOverride, nTransparencyLookup);
        }

        private void FixSubmesh()
        {
            var val = Read<M2Array>(0x1C);

            for (var i = 0u; i < val.Size; ++i)
            {
                var pos = (int)(val.Offset + i * 0x30);

                if (Read<short>(pos + 0x2) > 0)
                {
                    for (var j = pos + 0x2; j < pos + 0x14; ++j)
                        Write<byte>(0, j);
                    BadSubmesh.Add(i);
                }
                else if (Read<byte>(pos + 0x10) == 0)
                    Write<byte>(1, pos + 0x10);

                Write<byte>(0, 0x11);
            }
        }

        private void DuplicateTextureUnit(int start)
        {
            AddEmptyBytes(start + 0x18, 0x18);
            BlockCopy(start, Stream.ToArray(), start + 0x18, 0x18);
        }

        private void FixTexUnit(ref List<short> textureUnitLookup, ref List<ushort> blendmodeOverride, int nTransparencyLookup)
        {
            var texUnit = Read<M2Array>(0x24);

            for (var i = (int)texUnit.Offset; i < texUnit.Offset + texUnit.Size * 0x18; i += 0x18)
            {
                var shaderId        = Read<ushort>(i + 0x2);
                ushort shaderToSave = 0;

                var flags           = Read<byte>(i);
                var submesh         = Read<ushort>(i + 0x4);

                if (BadSubmesh.Contains(submesh))
                {
                    Write<uint>(0x8000, i + 0x2);
                    continue;
                }

                var textureCount = Read<ushort>(i + 0xE);

                flags &= 0x10;
                Write<byte>(flags, i);

                bool skipNextTu = false;
                if (shaderId >= 0x8000)
                {
                    var low = (ushort)(shaderId & 0xFF);

                    switch (low)
                    {
                        case 0:
                        case 3:
                        case 9:
                        case 17:
                        case 24:
                            {
                                skipNextTu = true;
                                textureCount = 2;
                                DuplicateTextureUnit(i);
                                texUnit.Size++;

                                Write<byte>(1, i + 0xE + 0x18);

                                // Use next Renderflags and Material Layer
                                var renderFlag = (ushort)(Read<ushort>(i + 0xA) + 1);
                                Write<ushort>(renderFlag, i + 0x18 + 0xA);
                                Write<short>(1, i + 0x18 + 0xC);

                                ushort blend1 = 1;
                                ushort blend2 = 4;
                                short[] tuLookup = { 0, -1 };

                                bool added = false;

                                for (ushort n = 0; n < blendmodeOverride.Count; n += 2)
                                {
                                    if (blendmodeOverride[n] == blend1 && blendmodeOverride[n + 1] == blend2)
                                    {
                                        shaderToSave = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    shaderToSave = (ushort)blendmodeOverride.Count;
                                    blendmodeOverride.Add(blend1);
                                    blendmodeOverride.Add(blend2);
                                }

                                added = false;
                                ushort texCoordToSave = 0;
                                for (ushort n = 0; n < textureUnitLookup.Count - 1; ++n)
                                {
                                    if (textureUnitLookup[n] == tuLookup[0] && textureUnitLookup[n + 1] == tuLookup[1])
                                    {
                                        texCoordToSave = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    var tulSize = textureUnitLookup.Count;
                                    if (tulSize > 1 && textureUnitLookup[tulSize - 1] == tuLookup[0])
                                    {
                                        texCoordToSave = (ushort)(tulSize - 1);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                    else
                                    {
                                        texCoordToSave = (ushort)tulSize;
                                        textureUnitLookup.Add(tuLookup[0]);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                }

                                // Texture coord combo
                                Write<ushort>(texCoordToSave, i + 0x12);
                                Write<ushort>(texCoordToSave, i + 0x18 + 0x12);

                                Write<ushort>(shaderToSave, i + 0x2);
                                Write<ushort>(shaderToSave, i + 0x2 + 0x18);
                            }
                            break;
                        case 1:
                        case 15:
                            {
                                skipNextTu = true;
                                textureCount = 1;
                                DuplicateTextureUnit(i);
                                texUnit.Size++;

                                Write<byte>(1, i + 0xE);        // Next TU has 1 Tex
                                Write<byte>(1, i + 0xE + 0x18); // Next TU has 1 Tex

                                var renderFlag      = (ushort)(Read<ushort>(i + 0xA) + 1);
                                var textureCombo    = (ushort)(Read<ushort>(i + 0x10) + 1);
                                var transCombo      = Read<ushort>(i + 0x14);

                                Write<ushort>(renderFlag, i + 0x18 + 0xA);
                                Write<short>(1, i + 0x18 + 0xC);
                                Write<ushort>(textureCombo, i + 0x18 + 0x10);

                                if (transCombo + 1 < nTransparencyLookup)
                                    transCombo++;

                                Write<ushort>(transCombo, i + 0x18 + 0x14);

                                short[] tuLookup = { 0, (short)(shaderId == 0x8001 ? -1 : 1) };

                                // No blend override
                                Write<ushort>(0, i + 0x2);
                                Write<ushort>(0, i + 0x2 + 0x18);

                                bool added = false;
                                ushort texCoordToSave = 0;
                                for (ushort n = 0; n < textureUnitLookup.Count - 1; ++n)
                                {
                                    if (textureUnitLookup[n] == tuLookup[0] && textureUnitLookup[n + 1] == tuLookup[1])
                                    {
                                        texCoordToSave = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    var tulSize = textureUnitLookup.Count;
                                    if (tulSize > 1 && textureUnitLookup[tulSize - 1] == tuLookup[0])
                                    {
                                        texCoordToSave = (ushort)(tulSize - 1);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                    else
                                    {
                                        texCoordToSave = (ushort)tulSize;
                                        textureUnitLookup.Add(tuLookup[0]);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                }

                                // Texture coord combo
                                Write<ushort>(texCoordToSave, i + 0x12);
                                Write<ushort>(texCoordToSave++, i + 0x12 + 0x18);
                            }
                            break;
                        case 2:
                            {
                                skipNextTu = true;
                                DuplicateTextureUnit(i);
                                texUnit.Size++;

                                Write<byte>(1, i + 0xE + 0x18);

                                // Use next Renderflags and Material Layer
                                var renderFlag = (ushort)(Read<ushort>(i + 0xA) + 1);
                                Write<ushort>(renderFlag, i + 0x18 + 0xA);
                                Write<short>(1, i + 0x18 + 0xC);

                                ushort blend1 = 1;
                                ushort blend2 = 3;
                                short[] tuLookup = { 0, -1 };

                                bool added = false;

                                for (ushort n = 0; n < blendmodeOverride.Count; n += 2)
                                {
                                    if (blendmodeOverride[n] == blend1 && blendmodeOverride[n + 1] == blend2)
                                    {
                                        shaderToSave = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    shaderToSave = (ushort)blendmodeOverride.Count;
                                    blendmodeOverride.Add(blend1);
                                    blendmodeOverride.Add(blend2);
                                }

                                Write<ushort>(shaderToSave, i + 0x2);
                                Write<ushort>(shaderToSave, i + 0x2 + 0x18);

                                added = false;
                                ushort texCoordToSave = 0;
                                for (ushort n = 0; n < textureUnitLookup.Count - 1; ++n)
                                {
                                    if (textureUnitLookup[n] == tuLookup[0] && textureUnitLookup[n + 1] == tuLookup[1])
                                    {
                                        texCoordToSave = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    var tulSize = textureUnitLookup.Count;
                                    if (tulSize > 1 && textureUnitLookup[tulSize - 1] == tuLookup[0])
                                    {
                                        texCoordToSave = (ushort)(tulSize - 1);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                    else
                                    {
                                        texCoordToSave = (ushort)tulSize;
                                        textureUnitLookup.Add(tuLookup[0]);
                                        textureUnitLookup.Add(tuLookup[1]);
                                    }
                                }

                                // Texture coord combo
                                Write<ushort>(texCoordToSave, i + 0x12);
                                Write<ushort>(texCoordToSave++, i + 0x12 + 0x18);
                            }
                            break;
                        case 5:
                        case 8:
                        case 10:
                        case 12:
                        case 16:
                        case 23:
                            shaderId = 0;
                            textureCount = 1;
                            break;
                        // Combiners_Mod_Mod
                        case 21:
                            shaderId = 0x4011;
                            textureCount = 2;
                            break;
                        // default: Combiners_Mod
                        default:
                            shaderId = 0x0010;
                            textureCount = 1;
                            break;
                    }
                }

                if (shaderId < 0x8000)
                {
                    ushort blend1 = (ushort)((shaderId >> 4) & 0x7);
                    ushort blend2 = (ushort)(shaderId & 0x7);

                    if (textureCount > 1 && (shaderId & 0x4000) != 0 && blend1 != 0 && blend2 != 0)
                    {
                        bool added = false;

                        for (ushort n = 0; n < blendmodeOverride.Count; n += 2)
                        {
                            if (blendmodeOverride[n] == blend1 && blendmodeOverride[n + 1] == blend2)
                            {
                                shaderToSave = n;
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            shaderToSave = (ushort)blendmodeOverride.Count;
                            blendmodeOverride.Add(blend1);
                            blendmodeOverride.Add(blend2);
                        }
                    }
                    else
                        textureCount = 1;

                    flags &= 0x10;
                    Write<byte>(flags, i);
                    Write<ushort>(shaderToSave, i + 0x2);

                    // Generate texture unit lookup
                    var tuLookup = new short[2];

                    if (textureCount == 1)
                    {
                        if ((shaderId & 0x80) != 0)
                            tuLookup[0] = -1;
                        else
                            tuLookup[0] = 0;

                        if (textureUnitLookup.Contains(tuLookup[0]))
                            Write<ushort>((ushort)textureUnitLookup.IndexOf(tuLookup[0]), i + 0x12);
                        else
                        {
                            Write<ushort>((ushort)textureUnitLookup.Count, i + 0x12);
                            textureUnitLookup.Add(tuLookup[0]);
                        }
                    }
                    else
                    {
                        if ((shaderId & 0x80) != 0)
                        {
                            tuLookup[0] = -1;
                            tuLookup[1] = (short)(((shaderId & 0x8) != 0) ? -1 : 0);
                        }
                        else
                        {
                            tuLookup[0] = 0;
                            tuLookup[1] = (short)(((shaderId & 0x8) != 0) ? -1 : (((shaderId & 0x4000) != 0) ? 1 : 0));
                        }

                        bool added = false;
                        for (ushort n = 0; n < textureUnitLookup.Count - 1; ++n)
                        {
                            if (textureUnitLookup[n] == tuLookup[0] && textureUnitLookup[n + 1] == tuLookup[1])
                            {
                                Write<ushort>(n, i + 0x12);
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            var tulSize = textureUnitLookup.Count;
                            if (tulSize > 1 && textureUnitLookup[tulSize - 1] == tuLookup[0])
                            {
                                Write<ushort>((ushort)(tulSize - 1), i + 0x12);
                                textureUnitLookup.Add(tuLookup[1]);
                            }
                            else
                            {
                                Write<ushort>((ushort)(tulSize), i + 0x12);
                                textureUnitLookup.Add(tuLookup[0]);
                                textureUnitLookup.Add(tuLookup[1]);
                            }
                        }
                    }
                }

                Write<ushort>(Math.Min(textureCount, (ushort)2), i + 0xE);

                if (skipNextTu)
                    i += 0x18;
            }

            Write<uint>(texUnit.Size, 0x24);
        }
    }
}
