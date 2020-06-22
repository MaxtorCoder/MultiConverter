using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using MultiConverter.Lib.Converters.Base;

namespace MultiConverter.Lib.Converters
{
    /// <summary>
    /// TODO : Take into account the ribbons, fix animation id, fix particles blend
    /// </summary>
    public class M2Converter : WowFile
    {
        public bool NeedFix { get; private set; } = true;

        private bool fixHelmOffset;
        private int particleCount;
        private uint particleOffset, texturesSize;
        private string modelName;
        private int animOffset, animCount, animLookupOffset, animLookupCount, textureCount, textureOffset, dataSize;

        private HashSet<uint> shiftedOfs                = new HashSet<uint>();
        private Dictionary<int, byte[]> multitextInfo   = new Dictionary<int, byte[]>();
        private List<SkinConverter> skins               = new List<SkinConverter>();

        private Dictionary<Texture, int> Textures       = new Dictionary<Texture, int>();

        public M2Converter(string m2, bool fix_helm) : base(m2)
        {
            fixHelmOffset = fix_helm;

            if (ReadUInt(0x4) <= 264)
                NeedFix = false;

            if (NeedFix)
            {
                // Read M2 file and check chunks to read them.
                using (var stream = new FileStream(m2, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var chunk   = new string(reader.ReadChars(4));
                        var size    = reader.ReadUInt32();

                        switch (chunk)
                        {
                            case "MD21":
                                // Save start offset of MD20
                                var offset                  = reader.BaseStream.Position;

                                // Skip Magic & Version
                                reader.ReadBytes(0x08);
                                var modelNameSize           = reader.ReadInt32();
                                var modelNameOfs            = reader.ReadUInt32() + 8;

                                reader.BaseStream.Position  = modelNameOfs;
                                modelName                   = new string(reader.ReadChars(modelNameSize)).Replace("\0", "");
                                reader.BaseStream.Position  = offset;

                                // Skip to M2Array<Texture>.
                                reader.ReadBytes(0x50);
                                textureCount                = reader.ReadInt32();
                                textureOffset               = reader.ReadInt32();

                                reader.BaseStream.Position = offset + size;
                                break;
                            case "TXID":
                                ReadTXID(reader, size);
                                break;
                            default:
                                reader.BaseStream.Position += size;
                                Console.WriteLine($"Skipping chunk: {chunk}");
                                break;
                        }
                    }

                    stream.Close();
                    reader.Close();
                }

                particleCount   = ReadInt(0x128);
                particleOffset  = ReadUInt(0x12C);
            }
        }

        public void RemoveLegionChunks()
        {
            int magic = ReadInt(0);

            if (magic != ChunkedWowFile.InvertedMagicToInt("MD20"))
            {
                int md20 = LocateMD20Chunk();
                int to_remove_start = 0;

                if (magic == ChunkedWowFile.InvertedMagicToInt("MD21"))
                    to_remove_start = ReadInt(4);

                if (md20 > 0)
                    RemoveBytes(0, md20);

                // remove extra chunks at the end
                if (to_remove_start > 0)
                    RemoveBytes(to_remove_start, Data.Length - to_remove_start);
            }
        }

        private void ReadTXID(BinaryReader reader, uint size)
        {
            for (var i = 0u; i < size / 4u; i++)
            {
                var textureId = reader.ReadUInt32();

                var filename = Listfile.LookupFilename(textureId, ".m2").Replace('/', '\\');
                var texture = new Texture
                {
                    Filename = filename + "\0\0",
                    FilenameLength = filename.Length
                };

                if (Textures.ContainsKey(texture))
                    Textures[texture]++;
                else
                    Textures.Add(texture, 1);
            }
        }

        public bool Fix()
        {
            //           file too small
            if (!NeedFix || Size() < 0x130)
                return false;

            RemoveLegionChunks();

            dataSize        = Data.Length;
            texturesSize    = CalculateTexturesSize();

            FixCamera();
            FixAnimations();
            FixSkins();

            // Nothing to do
            if (particleCount > 0)
            {
                int particleEnd = (int)particleOffset + (476 + 16) * particleCount;

                if (NeedParticleFix())
                {
                    AddEmptyBytes(particleEnd, 16 * particleCount);
                    for (int i = particleCount; i > 0; i--)
                    {
                        int pos = (i * 476) + (i - 1) * 16 + (int)particleOffset;
                        multitextInfo[i - 1] = new byte[16];
                        BlockCopy(pos, multitextInfo[i - 1], 0, 16);
                        RemoveBytes(pos, 16);
                    }
                }

                particleEnd = (int)particleOffset + 476 * particleCount;

                for (int i = 0; i < particleCount; i++)
                {
                    int pos = i * 476 + (int)particleOffset + 0x28;
                    char c = ReadChar(pos);

                    int flagsOfs = (int)(i * 476 + particleOffset + 4);
                    uint flags = ReadUInt(flagsOfs);
                    if (c > 4)
                    {
                        Data[pos] = 4;
                        //FixEmitterSpeed(i);
                    }

                    if ((flags & 0x800000) != 0)
                    {
                        FixGravity(i);
                    }


                    pos = i * 476 + (int)particleOffset + 22;
                    if ((flags & 0x10000000) != 0)
                    {
                        short val = ReadShort(pos);
                        short text0 = (short)(val & 0x1F);
                        short text1 = (short)((val & 0x3E0) >> 5);
                        short text2 = (short)((val & 0x7C00) >> 10);

                        WriteShort(pos, text0);
                    }

                    // 0x4000000 	do not throttle emission rate based on distance  => cause for high value emission rate ?
                    if ((flags & 0x4000000) != 0)
                    {
                        //FixEmitterSpeed(i);
                    }
                }

                for (int i = particleCount - 1; i >= 0; i--)
                {
                    if (BitConverter.ToUInt16(multitextInfo[i], 2) == 0xFF)
                    {
                        particleCount--;
                    }
                    else // particles added are at the end
                    {
                        break;
                    }
                }

                WriteInt(0x128, particleCount);

            }

            // update version
            WriteUInt(0x4, 264);

            if (fixHelmOffset)
                FixHelmOffset();

            if (Textures.Count >= 1)
                FixTXID();

            return true;
        }

        private void FixTXID()
        {
            var textureBlockSize = 4 * sizeof(uint);
            var textureKeys = Textures.Keys.ToList();

            // Write `0` block at the end of the file.
            AddEmptyBytes(Data.Length, (int)texturesSize + textureBlockSize);

            for (var i = 0; i < Textures.Count; ++i)
            {
                var texture = textureKeys[i];

                for (var j = 0; j < Textures[texture]; ++j)
                {
                    // TEX_COMPONENT_HARDCODED
                    var isHarcoded = ReadUInt(textureOffset) == 0;
                    if (isHarcoded)
                    {
                        // Write Filename Length and Offset of Filename;
                        WriteInt(textureOffset + 8, texture.FilenameLength);
                        WriteInt(textureOffset + 12, dataSize);

                        for (var x = 0; x < texture.Filename.Length; ++x)
                            WriteChar(dataSize + x, texture.Filename[x]);

                        dataSize += texture.Filename.Length;
                    }

                    // Block reading finished, add 4 * sizeof(uint)
                    textureOffset += textureBlockSize;
                }
            }
        }

        private void FixHelmOffset()
        {
            Regex r = new Regex(@"(.*(/|\\))?(helm)(et)?_.*_(be|dr|dw|gn|hu|ni|or|sc|ta|tr|sk|go)[mf]\.(m2)");
            if (!r.IsMatch(Path))
                return;

            for (int i = 0; i < ReadInt(0x2C); ++i)
                FixBoneHelmOffset(ReadInt(0x30) + 0x58 * i);
        }

        private void FixBoneHelmOffset(int bone_pos)
        {
            float x, z;
            int data_pos = Size();

            int flags = ReadInt(bone_pos + 0x4);
            flags |= 0x200;

            WriteInt(bone_pos + 0x4, flags);
            AddEmptyBytes(data_pos, 12 + 8 + 12);

            Data[bone_pos + 0x14] = 1;
            WriteInt(bone_pos + 0x18, data_pos);
            WriteInt(data_pos, 1);
            WriteInt(data_pos + 0x4, data_pos + 8);
            data_pos += 0xC;
            Data[bone_pos + 0x1C] = 1;
            WriteInt(bone_pos + 0x20, data_pos);
            WriteInt(data_pos, 1);
            WriteInt(data_pos + 0x4, data_pos + 8);
            data_pos += 0x8;

            string id = Path.Substring(Path.Length - 6, 3);

            switch (id)
            {
                default:
                    x = -0.0587258f;
                    z = -0.18623257f;
                    break;
                case "drf":
                    x = -0.0587258f;
                    z = -0.195f;
                    break;
                case "drm":
                    x = -0.0587258f;
                    z = -0.245f;
                    break;
                case "taf":
                    x = -0.13f;
                    z = -0.1f;
                    break;
                case "tam":
                    x = -0.2f;
                    z = -0.1f;
                    break;
                case "nim":
                    x = -0.09f;
                    z = -0.18f;
                    break;
                case "nif":
                    x = -0.08f;
                    z = -0.195f;
                    break;
                case "orf":
                    x = -0.08f;
                    z = -0.171f;
                    break;
                case "orm":
                    x = -0.13f;
                    z = -0.21f;
                    break;
                case "trf":
                    x = -0.0887258f;
                    z = -0.08623257f;
                    break;
                case "trm":
                    x = -0.13f;
                    z = -0.16f;
                    break;
                case "bef":
                    x = 0.01f;
                    z = -0.2f;
                    break;
                case "bem":
                    x = -0.08f;
                    z = -0.165f;
                    break;
                case "huf":
                    x = -0.09f;
                    z = -0.18f;
                    break;
                case "scm":
                    x = -0.12f;
                    z = -0.12623256f;
                    break;
                case "scf":
                    x = -0.01f;
                    z = -0.15f;
                    break;
                case "gnf":
                    x = -0.015f;
                    z = -0.263f;
                    break;
                case "gnm":
                    x = -0.009f;
                    z = -0.23f;
                    break;
                case "dwm":
                    x = -0.0227258f;
                    z = -0.1725f;
                    break;
                case "dwf":
                    x = 0.01f;
                    z = -0.195f;
                    break;
            }

            WriteFloat(data_pos, x);
            WriteFloat(data_pos + 0x8, z);
        }

        private void FixGravity(int i)
        {
            // gravity
            int pos = i * 476 + 0x90 + (int)particleOffset;
            int n = ReadInt(pos);
            int ofs = ReadInt(pos + 0x4);

            for (int j = 0; j < n; j++)
            {
                int nval = ReadInt(ofs + 0x8 * j);
                int ofsVal = ReadInt(ofs + 0x8 * j + 0x4);

                if (nval == 0 || ofsVal == 0)
                {
                    continue;
                }

                for (int k = ofsVal; k < ofsVal + 0x4 * nval; k += 0x4)
                {
                    float x = Data[k], y = Data[k + 1], z = Math.Abs(ReadShort(k + 2));
                    float x1 = x / 128.0f, y1 = y / 128.0f;
                    float z1 = (float)Math.Sqrt(1.0f - Math.Sqrt(x1 * x1 + y1 * y1));
                    float mag = z / 128.0f * 0.04238648f;
                    float result = x1 * mag;

                    WriteFloat(k, result);
                }
            }
        }

        // completely wrong !
        private void FixEmitterSpeed(int i)
        {
            // start of emission rate value
            int pos = i * 476 + 0xBC + (int)particleOffset;
            int n = ReadInt(pos);
            int ofs = ReadInt(pos + 0x4);

            for (int j = 0; j < n; j++)
            {
                int nval = ReadInt(ofs + 0x8 * j);
                int ofsVal = ReadInt(ofs + 0x8 * j + 0x4);

                if (nval == 0 || ofsVal == 0)
                {
                    continue;
                }

                for (int k = ofsVal; k < ofsVal + 0x4 * nval; k += 0x4)
                {
                    float x = Data[k], y = Data[k + 1], z = Math.Abs(ReadShort(k + 2));
                    float x1 = x / 128.0f, y1 = y / 128.0f;
                    float z1 = (float)Math.Sqrt(1.0f - Math.Sqrt(x1 * x1 + y1 * y1));
                    float mag = z / 128.0f * 0.04238648f;
                    float result = x1 * mag;
                    WriteFloat(k, result);
                }
            }
        }

        private void FixRenderFlags()
        {
            uint n = ReadUInt(0x70);
            uint ofs = ReadUInt(0x74);
            uint globalflags = ReadUInt(0x10);

            for (int i = 0; i < n; i++)
            {
                int pos = (int)ofs + i * 0x4;
                ushort flag = ReadUShort(pos);
                ushort blend = ReadUShort(pos + 0x2);

                if (blend > 6)
                {
                    blend = 4;
                    flag |= 0x5;
                }

                flag &= 0x1F;

                WriteUShort(pos, flag);
                WriteUShort(pos + 0x2, blend);
            }
        }

        private void FixBone()
        {
            uint boneCount = ReadUInt(0x2C);
            uint boneOfs = ReadUInt(0x30);

            for (uint i = 0; i < boneCount; i++)
            {
                int pos = (int)(0x58 * i + boneOfs);
                uint flag = ReadUInt(pos + 0x4);
                if (flag == 0x400)
                {
                    WriteUInt(pos + 0x4, 0x8);
                }
            }
        }

        private void FixTexUnit()
        {
            WriteUInt(0x88, 1);
            BlockCopy(0x94, Data, 0x8C, 4);
        }

        private void FixCamera()
        {
            uint nCam = ReadUInt(0x110);
            uint ofsCam = ReadUInt(0x114);

            int camEnd = (int)(ofsCam + 0x74 * nCam);
            AddEmptyBytes(camEnd, 0x10 * (int)nCam);

            for (int i = 0; i < nCam; i++)
            {
                int ofsFov = (int)(ofsCam + i * 0x64 + 0x4);
                AddEmptyBytes(ofsFov, 4);

                float fov = (ReadUInt((int)(ofsCam + i * 0x64)) == 0 ? 0.7f : 0.97f);
                WriteFloat(ofsFov, fov);

                int pos = (int)(ofsCam + ((i + 1) * 0x64));
                RemoveBytes(pos, 0x14);
            }
        }

        /// <summary>
        /// Get the id
        /// </summary>
        /// <returns></returns>
        private Dictionary<ushort, ushort> GetTransparencyLookupTargetType()
        {
            Dictionary<ushort, ushort> dico = new Dictionary<ushort, ushort>();

            int ofsTrans = ReadInt(0x5C);
            int ofsTransLookup = ReadInt(0x94);
            int nTransLookup = ReadInt(0x90);

            for (ushort i = 0; i < nTransLookup; i++)
            {
                short id = ReadShort(ofsTransLookup + 2 * i);
                dico[i] = ReadUShort(ofsTrans + id * 0x14);
            }

            return dico;
        }

        private void FixSkins()
        {
            int n = ReadInt(0x70);
            int ofs = ReadInt(0x74);

            int n_transparency_lookup = ReadInt(0x90);

            string f = Path.Replace(".m2", "0");
            List<short> texture_unit_lookup = new List<short>();
            List<ushort> blend_override = new List<ushort>();
            for (int i = 0; i < ReadInt(0x44); i++)
            {
                string s = f + i + ".skin";
                if (!File.Exists(s))
                    continue;

                var sf = new SkinConverter(s);
                sf.Fix(ref texture_unit_lookup, ref blend_override, n_transparency_lookup);
                skins.Add(sf);
            }

            if (blend_override.Count > 0)
            {
                uint globalflags = ReadUInt(0x10);
                globalflags |= 0x8;
                WriteUInt(0x10, globalflags);

                int blend_override_pos = Data.Length;
                int n_blend_override = blend_override.Count;
                AddEmptyBytes(blend_override_pos, 2 * n_blend_override);

                for (int i = 0; i < n_blend_override; ++i)
                {
                    WriteUShort(blend_override_pos + 0x2 * i, blend_override[i]);
                }

                WriteInt(0x130, blend_override.Count);
                WriteInt(0x134, blend_override_pos);
            }

            int start = Data.Length;
            WriteInt(0x88, texture_unit_lookup.Count);
            WriteInt(0x8C, start);

            AddEmptyBytes(start, texture_unit_lookup.Count * sizeof(ushort));
            for (int i = 0; i < texture_unit_lookup.Count; ++i)
            {
                WriteShort(start + i * 2, texture_unit_lookup[i]);
            }

            // correct the renderflags given
            FixRenderFlags();
        }

        private short AnimationIndex(int anim_id)
        {
            for (int i = 0; i < animCount; i++)
                if (ReadInt(animOffset + i * 0x40) == anim_id)
                    return (short)i;

            return -1;
        }

        void ReplaceAnimLookup(short old_pos, ushort new_id, short new_pos)
        {
            // if new_id < nAnimLookups => animLookups[new_id] should be = old_pos

            if (animLookupCount > new_id && ReadShort(animLookupOffset + 0x2 * new_id) == old_pos)
                WriteShort(animLookupOffset + 0x2 * new_id, new_pos);
            else
            {
                for (int i = 0; i < animLookupCount; ++i)
                {
                    if (ReadShort(animLookupOffset + i * 0x2) == old_pos)
                    {
                        WriteShort(animLookupOffset + 0x2 * i, new_pos);
                        break;
                    }
                }
            }
            // log anim couldn't be fixed ?
        }

        /*
          41,SwimIdle         548
          42,Swim             556
          43,SwimLeft         552
          44,SwimRight        554
          45,SwimBackwards    560

          39,JumpEnd          572
          37,JumpStart        564
          187,JumpLandRun     574
         */

        private void FixAnimations()
        {
            animCount = ReadInt(0x1C);
            animOffset = ReadInt(0x20);
            animLookupCount = ReadInt(0x24);
            animLookupOffset = ReadInt(0x28);

            for (int i = 0; i < animCount; i++)
            {
                int p = animOffset + i * 0x40;
                ushort id = ReadUShort(p);

                if (id > 505) // max tlk anim id
                {
                    ushort anim = id;
                    switch (id)
                    {
                        case 564: anim = 37; break;
                        case 548: anim = 41; break;
                        case 556: anim = 42; break;
                        case 552: anim = 43; break;
                        case 554: anim = 44; break;
                        case 562: anim = 45; break;
                        case 572: anim = 39; break;
                        case 574: anim = 187; break;
                    }

                    if (id != anim)
                    {
                        ReplaceAnimLookup(AnimationIndex(anim), anim, (short)i);
                        WriteUShort(p, anim);
                    }
                }

                p += 0x1C;
                // fix speed
                WriteUInt(p, ReadUInt(p) & 0xFFFF); // 0x1FF
            }
        }

        private int LocateMD20Chunk()
        {
            for (int i = 0; i < Size() - 3; i++)
                if (Data[i] == 0x4D && Data[i + 1] == 0x44 && Data[i + 2] == 0x32 && Data[i + 3] == 0x30)
                    return i;

            // todo : add error log
            return 0;
        }

        private bool NeedParticleFix()
        {
            byte[] b = new byte[4];
            BlockCopy((int)particleOffset + 476, b, 0, 4);
            for (int i = 0; i < 4; i++)
                if (b[i] != 0xFF)
                    return true;

            return false;
        }

        private HashSet<ushort> StaticAnim()
        {
            HashSet<ushort> anims = new HashSet<ushort>();
            uint nUVAnim = ReadUInt(0x98);
            uint ofs = ReadUInt(0x9C);

            for (ushort t = 0; t < nUVAnim; t++)
                if (ReadShort((int)(ofs + t * 0x2)) == -1)
                    anims.Add(t);

            return anims;
        }

        private HashSet<ushort> GetOpaqueRenderFlags()
        {
            HashSet<ushort> rf = new HashSet<ushort>();

            uint n = ReadUInt(0x70);
            uint ofs = ReadUInt(0x74);

            for (ushort i = 0; i < n; i++)
            {
                ushort flag = ReadUShort((int)(ofs + i * 0x4));
                ushort blend = ReadUShort((int)(ofs + i * 0x4 + 0x2));
                if (flag == 0 || blend == 0)
                {
                    rf.Add(i);
                }
            }

            return rf;
        }

        #region Shift Offset

        /// <summary>
        /// Apply changes to the offsets if needed
        /// </summary>
        /// <param name="shift"></param>
        private void ShiftAllOfs(uint ofs, int shift)
        {
            // name
            ShiftOfs(ofs, 0xC, shift);

            int header_end = 0x12C + ((ReadInt(0x10) & 0x8) != 0 ? 8 : 0);

            // first offset ; last offset;
            for (uint i = 0x18; i <= header_end; i += 8)
            {
                // To skip the structure TheFloats
                if (i == 0xA0)
                    i = 0xDC - 8;
                else
                    ShiftOfs(ofs, i, shift);
            }

            ShiftBoneOfs(ofs, shift);
            ShiftTransparencyOfs(ofs, shift);
            ShiftTexAnimOfs(ofs, shift);
            ShiftAttachementOfs(ofs, shift);
            ShiftEventOfs(ofs, shift);
            ShiftCameraOfs(ofs, shift);
            ShiftColors(ofs, shift);
            ShiftTextureOfs(ofs, shift);

            // Shift all the offsets for the particles
            for (uint i = 0; i < particleCount; i++)
            {
                ShiftOfsForParticleStruct(ofs, particleOffset + i * 476, shift);
            }
        }

        private void ShiftTextureOfs(uint ofs, int shift)
        {
            uint nTex = ReadUInt(0x48);
            uint ofsTex = ReadUInt(0x4C);

            if (nTex == 0 || ofsTex == 0)
                return;

            for (int i = 0; i < nTex; i++)
            {
                int pos = (int)ofsTex + i * 0x10 + 0xC;
                uint curOfs = ReadUInt(pos);
                if (curOfs > ofs)
                {
                    curOfs = (uint)(curOfs + shift);
                    WriteUInt(pos, curOfs);
                }
            }
        }

        private void ShiftOfs(uint baseOfs, uint adr, int shift)
        {
            uint value = ReadUInt((int)adr);

            if (value > (uint)Size() - shift)
                return;

            uint nval = (uint)(value + shift);

            if (shiftedOfs.Contains(adr))
                return;
            shiftedOfs.Add(adr);

            bool needShift = value > baseOfs;

            if (needShift)
            {
                WriteUInt((int)adr, nval);
            }
        }

        private void ShiftOfs(uint baseOfs, uint adr, int shift, bool adrShift4 = false)
        {
            uint value = ReadUInt((int)adr);

            if (value > (uint)Size() - shift)
                return;

            uint nval = (uint)(value + shift);

            if (shiftedOfs.Contains(adr))
                return;

            shiftedOfs.Add(adr);

            bool needShift = value > baseOfs;

            if (needShift)
            {
                WriteUInt((int)adr, nval);
            }
            if (adrShift4 && value > 0)
            {
                ShiftOfs(baseOfs, (needShift ? nval : value) + 4, shift, false);
            }
        }

        private void ShiftABlock(uint ofs, uint start, int shift, bool timestampOnly = false)
        {
            uint nTSP = ReadUInt((int)start + 0x4);
            ShiftOfs(ofs, start + 0x8, shift);
            uint adr = ReadUInt((int)start + 0x8);
            for (uint i = 0; i < nTSP; i++)
                ShiftOfs(ofs, adr + i * 0x8 + 0x4, shift);

            if (timestampOnly)
                return;

            uint nVal = ReadUInt((int)start + 0xC);
            ShiftOfs(ofs, start + 0x10, shift, true);
            adr = ReadUInt((int)start + 0x10);
            for (uint i = 0; i < nVal; i++)
                ShiftOfs(ofs, adr + i * 0x8 + 0x4, shift);
        }

        private void ShiftFakeABlock(uint ofs, uint start, int shift)
        {
            ShiftABlock(ofs, start - 0x4, shift);
        }

        private void ShiftColors(uint ofs, int shift)
        {
            uint nColors = ReadUInt(0x48);
            uint ofsColors = ReadUInt(0x4C);

            for (uint i = 0; i < nColors; i++)
            {
                ShiftABlock(ofs, ofsColors + i * 0x28, shift);
                ShiftABlock(ofs, ofsColors + i * 0x28 + 0x14, shift);
            }
        }

        private void ShiftCameraOfs(uint ofs, int shift)
        {
            uint nCam = ReadUInt(0x110);
            uint ofsCam = ReadUInt(0x114);

            for (uint i = 0; i < nCam; i++)
            {
                uint pos = 0x64 * i + ofsCam;
                ShiftABlock(ofs, pos + 0x10, shift);
                ShiftABlock(ofs, pos + 0x30, shift);
                ShiftABlock(ofs, pos + 0x50, shift);
            }
        }

        private void ShiftEventOfs(uint ofs, int shift)
        {
            uint eventCount = ReadUInt(0x100);
            uint eventOfs = ReadUInt(0x104);

            for (uint i = 0; i < eventCount; i++)
            {
                uint pos = 0x24 * i + eventOfs + 0x18;
                ShiftABlock(ofs, pos, shift, true);
            }
        }

        private void ShiftAttachementOfs(uint ofs, int shift)
        {
            uint attachementCount = ReadUInt(0xF0);
            uint attachementOfs = ReadUInt(0xF4);

            for (uint i = 0; i < attachementCount; i++)
            {
                uint pos = 0x28 * i + attachementOfs;
                ShiftABlock(ofs, pos + 0x14, shift);
            }
        }

        private void ShiftTexAnimOfs(uint ofs, int shift)
        {
            uint texAnimCount = ReadUInt(0x60);
            uint texAnimOfs = ReadUInt(0x64);

            for (uint i = 0; i < texAnimCount; i++)
            {
                uint pos = 0x3C * i + texAnimOfs;
                ShiftABlock(ofs, pos, shift);
                ShiftABlock(ofs, pos + 0x14, shift);
                ShiftABlock(ofs, pos + 0x28, shift);
            }
        }

        private void ShiftBoneOfs(uint ofs, int shift)
        {
            uint boneCount = ReadUInt(0x2C);
            uint boneOfs = ReadUInt(0x30);

            for (uint i = 0; i < boneCount; i++)
            {
                uint pos = 0x58 * i + boneOfs;
                // 0x18, 0x20, 28, 34, 40, 48
                ShiftABlock(ofs, pos + 0x10, shift);
                ShiftABlock(ofs, pos + 0x24, shift);
                ShiftABlock(ofs, pos + 0x38, shift);
            }
        }

        private void ShiftTransparencyOfs(uint ofs, int shift)
        {
            uint transpCount = ReadUInt(0x58);
            uint transpOfs = ReadUInt(0x5C);

            for (uint i = 0; i < transpCount; i++)
            {
                uint pos = 0x14 * i + transpOfs;
                ShiftABlock(ofs, pos, shift);
            }
        }

        private void ShiftOfsForParticleStruct(uint ofs, uint pos, int shift)
        {
            ShiftOfs(ofs, pos + 0x1C, shift);
            ShiftOfs(ofs, pos + 0x24, shift);
            ShiftABlock(ofs, pos + 0x34, shift);
            ShiftABlock(ofs, pos + 0x48, shift);
            ShiftABlock(ofs, pos + 0x5C, shift);
            ShiftABlock(ofs, pos + 0x70, shift);
            ShiftABlock(ofs, pos + 0x84, shift);
            ShiftABlock(ofs, pos + 0x98, shift);
            ShiftABlock(ofs, pos + 0xB0, shift);
            ShiftABlock(ofs, pos + 0xC8, shift);
            ShiftABlock(ofs, pos + 0xDC, shift);
            ShiftABlock(ofs, pos + 0xF0, shift);

            ShiftFakeABlock(ofs, pos + 0x104, shift);
            ShiftFakeABlock(ofs, pos + 0x114, shift);
            ShiftFakeABlock(ofs, pos + 0x124, shift);
            ShiftFakeABlock(ofs, pos + 0x13C, shift);
            ShiftFakeABlock(ofs, pos + 0x14C, shift);

            ShiftABlock(ofs, pos + 0x1C8, shift);

        }
        #endregion

        #region Calculate Shit
        private uint CalculateTexturesSize()
        {
            var result = 0u;

            foreach (var texture in Textures)
                for (var i = 0; i < texture.Value; ++i)
                    result += (uint)texture.Key.FilenameLength + 1;

            return result;
        }
        #endregion

        public override void Save()
        {
            if (Valid)
                File.WriteAllBytes(Path, Data);

            foreach (var skin in skins)
                skin.Save();
        }
    }

    public struct Texture
    {
        public string Filename;
        public int FilenameLength;
    }
}
