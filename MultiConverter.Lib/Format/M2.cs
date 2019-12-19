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

        private Dictionary<M2Chunk, uint> Chunks = new Dictionary<M2Chunk, uint>();
        private Dictionary<string, int> TexturePaths = new Dictionary<string, int>();

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
            //TODO: FixSkin()

            if (Particles.Size > 0)
            {

            }

            return true;
        }

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
