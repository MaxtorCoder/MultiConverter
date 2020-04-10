using MultiConverter.Lib.Constants;
using MultiConverter.Lib.Interface;
using MultiConverter.Lib.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiConverter.Lib.Format
{
    public class M2 : IWowFile, IConverter
    {
        private string ModelName;
        private int DataSize, TextureSize;
        private bool FixHelmOffset          = false;
        private bool NeedFix                = true;

        private M2Array Textures            = new M2Array();
        private M2Array Animations          = new M2Array();
        private M2Array AnimationLookup     = new M2Array();
        private M2Array Particles           = new M2Array();

        private Dictionary<int, byte[]> multitextInfo   = new Dictionary<int, byte[]>();
        private Dictionary<M2Chunk, uint> Chunks        = new Dictionary<M2Chunk, uint>();
        private Dictionary<string, int> TexturePaths    = new Dictionary<string, int>();
        private List<SKIN> Skins                        = new List<SKIN>();

        public M2(string path, bool fixHelm) : base(path)
        {
            FixHelmOffset = fixHelm;

            if (Read<uint>(0xC) <= 264)
                NeedFix = false;

            Reader.BaseStream.Position = 0;
            while (Reader.BaseStream.Position < Reader.BaseStream.Length)
            {
                var chunk   = (M2Chunk)Read<uint>();
                var size    = Read<uint>();

                if (chunk != M2Chunk.MD21)
                    Chunks.Add(chunk, size);

                switch (chunk)
                {
                    case M2Chunk.MD21:
                        // Skip to modelname Array
                        var modelName = Read<M2Array>(0x10);

                        // Read modelname
                        Reader.BaseStream.Position = modelName.Offset + 8;
                        ModelName = new string(Reader.ReadChars((int)modelName.Size));

                        // Read M2Array<Texture>.
                        Textures = Read<M2Array>(0x58);

                        // Skip entire MD20 chunk.
                        Reader.BaseStream.Position = 0x8 + size;
                        break;
                    case M2Chunk.TXID:
                        ReadTXID(size);
                        break;
                    default:
                        Reader.BaseStream.Position += size;
                        Console.WriteLine($"Skipping chunk: {chunk}");
                        break;
                }
            }

            DataSize    = Size - (int)CalculateChunksSize() - 8;
            TextureSize = CalculateTexturesSize();

            // Read the Particle M2 Array.
            Particles = Read<M2Array>(0x130);
        }

        public void RemoveChunks()
        {
            var magic = (M2Chunk)Read<uint>(0);

            if (magic != M2Chunk.MD20)
            {
                var toRemoveStart = 0;

                if (magic == M2Chunk.MD21)
                    toRemoveStart = Read<int>(0x4);

                // Remove MD21 chunk
                RemoveBytes(0, 8);

                // Remove extra chunks at the end
                if (toRemoveStart > 0)
                    RemoveBytes(toRemoveStart, Size - toRemoveStart);
            }
        }

        public bool Fix()
        {
            if (!NeedFix || Size < 0x130)
                return false;

            RemoveChunks();

            FixTXID();
            FixCamera();
            FixAnimations();
            FixSkins();
            FixParticles();

            // Update M2 Version
            Write<uint>(264, 0x4);

            return true;
        }

        #region SKIN
        private void FixSkins()
        {
            var textureUnitLookup   = new List<short>();
            var blendOverride       = new List<ushort>();
            var transLookup         = Read<M2Array>(0x90);

            var f                   = Path.Replace(".m2", "0");
            var nViews              = Read<uint>(0x44);
            for (var i = 0; i < nViews; ++i)
            {
                var skinPath = f + i + ".skin";
                if (!File.Exists(skinPath))
                    continue;

                var skin = new SKIN(skinPath);
                skin.Fix(ref textureUnitLookup, ref blendOverride, (int)transLookup.Size);
                Skins.Add(skin);
            }

            if (blendOverride.Count > 0)
            {
                var globalFlags = Read<uint>(0x10) | 0x8;
                Write<uint>(globalFlags, 0x10);

                var blendOverridePos = Size;
                var blendOverrideCnt = blendOverride.Count;
                AddEmptyBytes(blendOverridePos, 2 * blendOverrideCnt);

                for (var i = 0; i < blendOverrideCnt; ++i)
                    Write<ushort>(blendOverride[i], blendOverridePos + 0x2 * i);

                Write<int>(blendOverrideCnt, 0x130);
                Write<int>(blendOverridePos, 0x134);
            }

            var start = Size;
            Write<int>(textureUnitLookup.Count, 0x88);
            Write<int>(start, 0x8C);

            AddEmptyBytes(start, textureUnitLookup.Count * 2);
            for (var i = 0; i < textureUnitLookup.Count; ++i)
                Write<short>(textureUnitLookup[i], start + i * 2);

            // Correct the renderflags
            FixRenderFlags();
        }

        private void FixRenderFlags()
        {
            var materials = Read<M2Array>(0x70);
            
            for (var i = 0; i < materials.Size; ++i)
            {
                var pos = (int)materials.Offset + i * 0x4;
                var flag = Read<ushort>(pos);
                var blend = Read<ushort>(pos + 0x2);

                if (blend > 6)
                {
                    blend = 4;
                    flag |= 0x5;
                }

                flag &= 0x1F;

                Write<ushort>(flag, pos);
                Write<ushort>(blend, pos + 2);
            }
        }
        #endregion
        #region Particles
        private void FixParticles()
        {
            if (Particles.Size > 0)
            {
                int particleEnd = (int)Particles.Offset + (476 + 16) * (int)Particles.Size;

                if (NeedParticleFix())
                {
                    AddEmptyBytes(particleEnd, 16 * (int)Particles.Size);
                    for (var i = (int)Particles.Size; i > 0; --i)
                    {
                        var pos = (i * 476) + (i - 1) * 16 + (int)Particles.Offset;
                        multitextInfo[i - 1] = new byte[16];
                        BlockCopy(pos, multitextInfo[i - 1], 0, 16);
                        RemoveBytes(pos, 16);
                    }
                }

                particleEnd = (int)Particles.Offset + 476 * (int)Particles.Size;
                for (var i = 0; i < Particles.Size; ++i)
                {
                    var pos = i * 476 + (int)Particles.Offset + 0x28;
                    var c = Read<char>(pos);

                    var flagOfs = (int)(i * 476 + Particles.Offset + 4);
                    var flags = Read<uint>(flagOfs);

                    if (c > 4)
                        Write<int>(4, pos);

                    if ((flags & 0x800000) != 0)
                        FixGravity(i);

                    pos = i * 476 + (int)Particles.Offset + 22;
                    if ((flags & 0x10000000) != 0)
                    {
                        var val = Read<short>(pos);
                        var txt0 = (short)(val & 0x1F);
                        var txt1 = (short)((val & 0x3E0) >> 5);
                        var txt2 = (short)((val & 0x7C00) >> 10);

                        Write<short>(txt0, pos);
                    }

                    // 0x40000000 -  EmitterSpeed
                }

                for (var i = (int)Particles.Size - 1; i >= 0; --i)
                {
                    if (BitConverter.ToUInt16(multitextInfo[i], 2) == 0xFF)
                        Particles.Size--;
                    else
                        break;
                }

                Write<uint>(Particles.Size, 0x128);
            }
        }

        private bool NeedParticleFix()
        {
            var b = new byte[4];
            BlockCopy((int)Particles.Offset + 476, b, 0, 4);

            for (var i = 0; i < 4; ++i)
                if (b[i] != 0xFF)
                    return true;

            return false;
        }

        private void FixGravity(int i)
        {
            var pos     = i * 476 + 0x90 + (int)Particles.Offset;
            var array   = Read<M2Array>(pos);

            for (var j = 0; j < array.Size; ++j)
            {
                var val = Read<M2Array>((int)array.Offset + 0x8 * j);

                if (val.Size == 0 || val.Offset == 0)
                    continue;

                for (var k = val.Offset; k < val.Offset + 0x4 * val.Size; k += 0x4)
                {
                    var x = Read<float>((int)k);
                    var y = Read<float>((int)k + 1);
                    var z = Math.Abs(Read<short>((int)k + 2));

                    var x1 = x / 128.0f;
                    var y1 = y / 128.0f;
                    var z1 = (float)Math.Sqrt(1.0f - Math.Sqrt(x1 * x1 + y1 * y1));
                    var mag = z / 128.0f * 0x04238648f;

                    var result = x1 * mag;
                    Write<float>(result, (int)k);
                }
            }
        }
        #endregion
        #region Textures
        private void FixTXID()
        {
            int textureBlockSize = 16;

            // Write `0` block at the end of the file.
            AddEmptyBytes(DataSize, TextureSize + textureBlockSize);

            for (var i = 0; i < Textures.Size; ++i)
            {
                // TEX_COMPONENT_HARDCODED
                var isHarcoded = Read<uint>((int)Textures.Offset) == 0;
                if (isHarcoded)
                {
                    var texture = TexturePaths.First();

                    // Write Filename Length and Offset of Filename;
                    Write<int>(texture.Value, (int)Textures.Offset + 8);
                    Write<int>(DataSize, (int)Textures.Offset + 12);

                    for (var j = 0; j < texture.Value; ++j)
                        Write<char>(texture.Key[j], DataSize + j);

                    DataSize += texture.Value + 2;
                    TexturePaths.Remove(texture.Key);
                }

                // Block reading finished, add 16 (4 * sizeof(uint))
                Textures.Offset += (uint)textureBlockSize;
            }
        }

        private void ReadTXID(uint size)
        {
            for (var i = 0; i < size / 4; ++i)
            {
                var textureId = Read<uint>();

                var filename = Listfile.LookupFilename(textureId, ".m2", ModelName);
                if (filename != string.Empty)
                    TexturePaths.Add(filename + "\0\0", filename.Length);
                else
                    Console.WriteLine("DBC Defined Texture!");
            }
        }
        #endregion
        #region Camera
        private void FixCamera()
        {
            var Camera = Read<M2Array>(0x110);
            var CameraEnd = (int)(Camera.Offset + 0x74 * Camera.Size);
            
            // Allocate a new area for Cameras
            AddEmptyBytes(CameraEnd, 0x10 * (int)Camera.Size);

            for (var i = 0; i < Camera.Size; ++i)
            {
                var ofsFov = (int)(Camera.Offset + i * 0x64 + 0x4);
                AddEmptyBytes(ofsFov, 4);

                var FOV = (Read<uint>((int)(Camera.Offset + i * 0x64)) == 0 ? .7f : .97f);
                Write<float>(FOV, ofsFov);

                var removePos = (int)(Camera.Offset + ((i + 1) * 0x64));
                RemoveBytes(removePos, 0x14);
            }
        }
        #endregion
        #region Animation
        private short AnimationIndex(int anim_id)
        {
            for (int i = 0; i < Animations.Size; i++)
                if (Read<int>((int)Animations.Offset + i * 0x40) == anim_id)
                    return (short)i;

            return -1;
        }

        private void FixAnimations()
        {
            Animations      = Read<M2Array>(0x1C);
            AnimationLookup = Read<M2Array>(0x24);

            for (var i = 0; i < Animations.Size; ++i)
            {
                var offset = (int)Animations.Offset + i * 0x40;
                var AnimId = Read<ushort>(offset);

                if (AnimId > 505) //< Max TLK Anim Id
                {
                    var newAnimId = AnimId;
                    switch (AnimId)
                    {
                        case 564: newAnimId = 37; break;
                        case 548: newAnimId = 41; break;
                        case 556: newAnimId = 42; break;
                        case 552: newAnimId = 43; break;
                        case 554: newAnimId = 44; break;
                        case 562: newAnimId = 45; break;
                        case 572: newAnimId = 39; break;
                        case 574: newAnimId = 187; break;
                    }

                    if (AnimId != newAnimId)
                    {
                        ReplaceAnimLookup(AnimationIndex(newAnimId), newAnimId, (short)i);
                        Write<ushort>(newAnimId, offset);
                    }
                }

                offset += 0x1C;

                // Fix Animation Speed
                Write<uint>(Read<uint>(offset) & 0xFFFF, offset);
            }
        }

        private void ReplaceAnimLookup(short oldPos, ushort newId, short newPos)
        {
            if (AnimationLookup.Size > newId && Read<short>((int)AnimationLookup.Offset + 0x2 * newId) == oldPos)
                Write<short>(newPos, (int)AnimationLookup.Offset + 0x2 * newId);
            else
            {
                for (var i = 0; i < AnimationLookup.Size; ++i)
                {
                    if (Read<short>((int)AnimationLookup.Offset + i * 0x2) == oldPos)
                    {
                        Write<short>(newPos, (int)AnimationLookup.Offset * 0x2 + i);
                        break;
                    }
                }
            }
        }
        #endregion
        #region Calculate Shit
        private uint CalculateChunksSize()
        {
            uint result = 0;

            foreach (var chunk in Chunks)
                result += chunk.Value + 8; //< Calculate Chunk- and Size Identifiers

            return result;
        }

        private int CalculateTexturesSize()
        {
            int result = 0;

            foreach (var texture in TexturePaths)
                result += texture.Value + 1;

            return result;
        }
        #endregion
    }
}
