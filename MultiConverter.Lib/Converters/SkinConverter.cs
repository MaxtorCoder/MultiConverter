using MultiConverter.Lib.Converters.Base;
using System;
using System.Collections.Generic;

namespace MultiConverter.Lib.Converters
{
    public class SkinConverter : WowFile
    {
        private HashSet<uint> badsubmesh;

        public SkinConverter(string skin) : base(skin)
        {
            badsubmesh = new HashSet<uint>();
        }

        public void Fix(ref List<short> texture_unit_lookup, ref List<ushort> blendmode_override, int n_transparency_lookup)
        {
            FixSubmesh();
            FixTexUnit(ref texture_unit_lookup, ref blendmode_override, n_transparency_lookup);
        }

        private void FixSubmesh()
        {
            uint n = ReadUInt(0x1C);
            uint ofs = ReadUInt(0x20);

            for (uint i = 0; i < n; i++)
            {
                int pos = (int)(ofs + i * 0x30);

                if (ReadUShort(pos + 0x2) > 0)
                {
                    for (int k = pos + 0x2; k < pos + 0x14; ++k)
                    {
                        Data[k] = 0;
                    }
                    badsubmesh.Add(i);
                }
                else if (Data[pos + 0x10] == 0)
                {
                    Data[pos + 0x10] = 1;
                }
                Data[pos + 0x11] = 0;
            }
        }

        private void DuplicateTextureUnit(int start)
        {
            AddEmptyBytes(start + 0x18, 0x18);
            Buffer.BlockCopy(Data, start, Data, start + 0x18, 0x18);
        }

        private void FixTexUnit(ref List<short> texture_unit_lookup, ref List<ushort> blendmode_override, int n_transparency_lookup)
        {
            uint nTexUnit = ReadUInt(0x24);
            uint ofsTextUnit = ReadUInt(0x28);

            for (int i = (int)ofsTextUnit; i < ofsTextUnit + nTexUnit * 0x18; i += 0x18)
            {
                ushort shader_id = ReadUShort(i + 0x2), shader_to_save = 0;
                byte flags = Data[i];

                ushort submesh = ReadUShort((int)i + 0x4);

                if (badsubmesh.Contains(submesh))
                {
                    WriteUInt((int)i + 0x2, 0x8000);
                    continue;
                }

                ushort texture_count = ReadUShort(i + 0xE);
                Data[i] &= 0x10;

                bool skip_next_tu = false;

                if (shader_id >= 0x8000)
                {
                    ushort low = (ushort)(shader_id & 0xFF);

                    switch (low)
                    {
                        case 0:
                        case 3:
                        case 9:
                        case 17:
                        case 24:
                            {
                                skip_next_tu = true;
                                texture_count = 2;
                                DuplicateTextureUnit(i);
                                nTexUnit++;

                                Data[i + 0xE + 0x18] = 1; // next tu has 1 tex

                                // use next renderflags
                                WriteUShort(i + 0x18 + 0xA, (ushort)(ReadUShort(i + 0xA) + 1));
                                // material layer
                                WriteShort(i + 0x18 + 0xC, 1);

                                ushort blend_1 = 1;
                                ushort blend_2 = 4;
                                short[] tu_lookup = { 0, -1 };

                                bool added = false;

                                for (ushort n = 0; n < blendmode_override.Count; n += 2)
                                {
                                    if (blendmode_override[n] == blend_1 && blendmode_override[n + 1] == blend_2)
                                    {
                                        shader_to_save = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    shader_to_save = (ushort)blendmode_override.Count;
                                    blendmode_override.Add(blend_1);
                                    blendmode_override.Add(blend_2);
                                }

                                added = false;
                                ushort tex_coord_to_save = 0;
                                for (ushort n = 0; n < texture_unit_lookup.Count - 1; ++n)
                                {
                                    if (texture_unit_lookup[n] == tu_lookup[0] && texture_unit_lookup[n + 1] == tu_lookup[1])
                                    {
                                        tex_coord_to_save = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    int tul_size = texture_unit_lookup.Count;
                                    if (tul_size > 1 && texture_unit_lookup[tul_size - 1] == tu_lookup[0])
                                    {
                                        tex_coord_to_save = (ushort)(tul_size - 1);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                    else
                                    {
                                        tex_coord_to_save = (ushort)(tul_size);
                                        texture_unit_lookup.Add(tu_lookup[0]);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                }

                                // texture coord combo
                                WriteUShort(i + 0x12, tex_coord_to_save);
                                WriteUShort(i + 0x18 + 0x12, tex_coord_to_save);

                                WriteUShort(i + 0x2, shader_to_save);
                                WriteUShort(i + 0x2 + 0x18, shader_to_save);
                            }
                            break;
                        case 1:
                        case 15:
                            {
                                skip_next_tu = true;
                                texture_count = 1;
                                DuplicateTextureUnit(i);
                                nTexUnit++;

                                Data[i + 0xE] = 1;        // this tu has 1 tex
                                Data[i + 0xE + 0x18] = 1; // next tu has 1 tex

                                // use next renderflags
                                WriteUShort(i + 0x18 + 0xA, (ushort)(ReadUShort(i + 0xA) + 1));
                                // material layer
                                WriteShort(i + 0x18 + 0xC, 1);
                                // texture combo
                                WriteUShort(i + 0x18 + 0x10, (ushort)(ReadUShort(i + 0x10) + 1));
                                // transparency combo
                                ushort transparency_combo = (ushort)(ReadUShort(i + 0x14));
                                if (transparency_combo + 1 < n_transparency_lookup)
                                {
                                    transparency_combo++;
                                }

                                WriteUShort(i + 0x18 + 0x14, transparency_combo);

                                short[] tu_lookup = { 0, (short)(shader_id == 0x8001 ? -1 : 1) };

                                // no blend override
                                WriteUShort(i + 0x2, 0);
                                WriteUShort(i + 0x2 + 0x18, 0);


                                bool added = false;
                                ushort tex_coord_to_save = 0;
                                for (ushort n = 0; n < texture_unit_lookup.Count - 1; ++n)
                                {
                                    if (texture_unit_lookup[n] == tu_lookup[0] && texture_unit_lookup[n + 1] == tu_lookup[1])
                                    {
                                        tex_coord_to_save = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    int tul_size = texture_unit_lookup.Count;
                                    if (tul_size > 1 && texture_unit_lookup[tul_size - 1] == tu_lookup[0])
                                    {
                                        tex_coord_to_save = (ushort)(tul_size - 1);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                    else
                                    {
                                        tex_coord_to_save = (ushort)(tul_size);
                                        texture_unit_lookup.Add(tu_lookup[0]);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                }

                                // texture coord combo
                                WriteUShort(i + 0x12, tex_coord_to_save);
                                tex_coord_to_save++;
                                WriteUShort(i + 0x18 + 0x12, tex_coord_to_save);
                            }
                            break;
                        case 2:
                            {
                                skip_next_tu = true;
                                DuplicateTextureUnit(i);
                                nTexUnit++;

                                Data[i + 0xE + 0x18] = 1; // next tu has 1 tex

                                // use next renderflags
                                WriteUShort(i + 0x18 + 0xA, (ushort)(ReadUShort(i + 0xA) + 1));
                                // material layer
                                WriteShort(i + 0x18 + 0xC, 1);

                                ushort blend_1 = 1;
                                ushort blend_2 = 3;
                                short[] tu_lookup = { 0, -1 };

                                bool added = false;

                                for (ushort n = 0; n < blendmode_override.Count; n += 2)
                                {
                                    if (blendmode_override[n] == blend_1 && blendmode_override[n + 1] == blend_2)
                                    {
                                        shader_to_save = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    shader_to_save = (ushort)blendmode_override.Count;
                                    blendmode_override.Add(blend_1);
                                    blendmode_override.Add(blend_2);
                                }

                                WriteUShort(i + 0x2, shader_to_save);
                                WriteUShort(i + 0x2 + 0x18, shader_to_save);

                                added = false;
                                ushort tex_coord_to_save = 0;
                                for (ushort n = 0; n < texture_unit_lookup.Count - 1; ++n)
                                {
                                    if (texture_unit_lookup[n] == tu_lookup[0] && texture_unit_lookup[n + 1] == tu_lookup[1])
                                    {
                                        tex_coord_to_save = n;
                                        added = true;
                                        break;
                                    }
                                }

                                if (!added)
                                {
                                    int tul_size = texture_unit_lookup.Count;
                                    if (tul_size > 1 && texture_unit_lookup[tul_size - 1] == tu_lookup[0])
                                    {
                                        tex_coord_to_save = (ushort)(tul_size - 1);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                    else
                                    {
                                        tex_coord_to_save = (ushort)(tul_size);
                                        texture_unit_lookup.Add(tu_lookup[0]);
                                        texture_unit_lookup.Add(tu_lookup[1]);
                                    }
                                }

                                // texture coord combo
                                WriteUShort(i + 0x12, tex_coord_to_save);
                                WriteUShort(i + 0x18 + 0x12, tex_coord_to_save);
                            }
                            break;
                        case 5:
                        case 8:
                        case 10:
                        case 12:
                        case 16:
                        case 23:
                            shader_id = 0;
                            texture_count = 1;
                            break;
                        // Combiners_Mod_Mod
                        case 21:
                            shader_id = 0x4011;
                            texture_count = 2;
                            break;
                        // default: Combiners_Mod
                        default:
                            shader_id = 0x0010;
                            texture_count = 1;
                            break;
                    }
                }

                if (shader_id < 0x8000)
                {
                    ushort blend_1 = (ushort)((shader_id >> 4) & 0x7);
                    ushort blend_2 = (ushort)(shader_id & 0x7);

                    //                      T2
                    if (texture_count > 1 && (shader_id & 0x4000) != 0 && blend_1 != 0 && blend_2 != 0)
                    {
                        bool added = false;

                        for (ushort n = 0; n < blendmode_override.Count; n += 2)
                        {
                            if (blendmode_override[n] == blend_1 && blendmode_override[n + 1] == blend_2)
                            {
                                shader_to_save = n;
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            shader_to_save = (ushort)blendmode_override.Count;
                            blendmode_override.Add(blend_1);
                            blendmode_override.Add(blend_2);
                        }
                    }
                    else
                    {
                        texture_count = 1;
                    }

                    Data[i] &= 0x10;
                    WriteUShort(i + 0x2, shader_to_save);

                    // generate texture unit lookup
                    short[] tu_lookup = new short[2];

                    if (texture_count == 1)
                    {
                        if ((shader_id & 0x80) != 0)
                        {
                            tu_lookup[0] = -1;
                        }
                        else
                        {
                            tu_lookup[0] = 0;
                        }

                        if (texture_unit_lookup.Contains(tu_lookup[0]))
                        {
                            WriteUShort(i + 0x12, (ushort)texture_unit_lookup.IndexOf(tu_lookup[0]));
                        }
                        else
                        {
                            WriteUShort(i + 0x12, (ushort)texture_unit_lookup.Count);
                            texture_unit_lookup.Add(tu_lookup[0]);
                        }
                    }
                    else
                    {
                        if ((shader_id & 0x80) != 0)
                        {
                            tu_lookup[0] = -1;
                            // check if wotlk need to use Env_T2 rather than Env_T1
                            tu_lookup[1] = (short)(((shader_id & 0x8) != 0) ? -1 : 0);
                        }
                        else
                        {
                            tu_lookup[0] = 0;
                            tu_lookup[1] = (short)(((shader_id & 0x8) != 0) ? -1 : (((shader_id & 0x4000) != 0) ? 1 : 0));
                        }

                        bool added = false;
                        for (ushort n = 0; n < texture_unit_lookup.Count - 1; ++n)
                        {
                            if (texture_unit_lookup[n] == tu_lookup[0] && texture_unit_lookup[n + 1] == tu_lookup[1])
                            {
                                WriteUShort(i + 0x12, n);
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            int tul_size = texture_unit_lookup.Count;
                            if (tul_size > 1 && texture_unit_lookup[tul_size - 1] == tu_lookup[0])
                            {
                                WriteUShort(i + 0x12, (ushort)(tul_size - 1));
                                texture_unit_lookup.Add(tu_lookup[1]);
                            }
                            else
                            {
                                WriteUShort(i + 0x12, (ushort)tul_size);
                                texture_unit_lookup.Add(tu_lookup[0]);
                                texture_unit_lookup.Add(tu_lookup[1]);
                            }
                        }
                    }
                }

                WriteUShort(i + 0xE, Math.Min(texture_count, (ushort)2));

                if (skip_next_tu)
                {
                    i += 0x18;
                }
            }

            WriteUInt(0x24, nTexUnit);
        }
    }
}
