using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverterLib
{
    public class WMORootConverter : ChunkedWowFile, IConverter
    {
        private string wmoName;

        private List<string> FilenamePosition = new List<string>();
        private List<MOMT> MOMTEntries = new List<MOMT>();
        private List<string> MODIEntries = new List<string>();

        public WMORootConverter(string wmo) : base(wmo)
        {
            wmoName = wmo;
        }

        public bool Fix()
        {
            if (!Valid || Size() < 0x5C)
                return false;

            int pos = 0;// 0x54 + ReadInt(0x58) + 0x8;

            pos = SkipChunk(pos, "MVER");
            pos = SkipChunk(pos, "MOHD");

            var motx = MagicToInt("MOTX");
            var ofs = ChunksOfs(pos, motx);
            if (ofs.ContainsKey(motx))
            {
                pos = SkipChunk(pos, "MOTX");
                ReadMOMT(pos);
                pos = WriteMOMT(pos);
            }
            else
            {
                ReadMOMT(pos);
                var motxSize = CalculateTextureSize(FilenamePosition);
                Console.WriteLine($"MOTXSize: {motxSize}");
                AddEmptyBytes(pos, (int)motxSize + 8);
                WriteHeaderMagic(pos, "MOTX");
                WriteUInt(pos + 4, motxSize);

                pos += 8;
                foreach (var texture in FilenamePosition)
                {
                    var newFilename = texture.ToUpper();
                    for (var j = 0; j < newFilename.Length; ++j)
                        WriteChar(pos + j, newFilename[j]);
                    pos += newFilename.Length;
                }

                pos = WriteMOMT(pos);
            }

            pos = SkipChunk(pos, "MOGN");
            pos = FixMOGI(pos);// MOGI

            int mosb = MagicToInt("MOSB");
            ofs = ChunksOfs(pos, mosb);
            if (!ofs.ContainsKey(mosb))
            {
                AddEmptyBytes(pos, 0xC);
                WriteInt(pos, mosb);
                WriteInt(pos + 0x4, 4);
                pos += 0xC;
            }
            else
                pos = SkipChunk(pos, "MOSB");

            pos = SkipChunk(pos, "MOPV");
            pos = SkipChunk(pos, "MOPT");
            pos = SkipChunk(pos, "MOPR");

            int pos_molt = pos;
            pos = SkipChunk(pos, "MOLT");
            // fix nLights
            WriteInt(0x20, ReadInt(pos_molt + 0x4) / 0x30);

            pos = SkipChunk(pos, "MODS");

            int modi = MagicToInt("MODI");
            ofs = ChunksOfs(pos, modi);
            if (ofs.ContainsKey(modi))
            {
                pos += 4;
                var size = ReadUInt(pos);

                for (var i = 0; i < size / 4; ++i)
                {
                    pos += 4;
                    var filedataid = ReadUInt(pos);
                    var filename = Listfile.LookupFilename(filedataid, ".wmo", wmoName, "m2");
                    if (!MODIEntries.Contains(filename + "\0\0"))
                        MODIEntries.Add(filename + "\0\0");
                }

                pos += 4;

                var modiSize = CalculateTextureSize(MODIEntries);
                AddEmptyBytes(pos, (int)modiSize + 8);
                WriteHeaderMagic(pos, "MODN");
                WriteUInt(pos + 0x4, modiSize);

                pos += 0x8;
                foreach (var filename in MODIEntries)
                {
                    var upperFilename = filename.ToUpper();
                    for (var i = 0; i < upperFilename.Length; ++i)
                        WriteChar(pos + i, upperFilename[i]);
                    pos += upperFilename.Length;
                }
            }

            pos = FixMODD(pos);// MODD
            pos = SkipChunk(pos, "MFOG");
            pos = SkipMCVP(pos); // Optional chunk

            return true;
        }

        // TODO: investigate why I wrote that
        private int FixMFOG(int pos)
        {
            RemoveUnwantedChunksUntil(pos, "MFOG");
            int size = ReadInt(pos + 0x4);
            int nMFOG = size / 0x30;
            pos += 0x8;

            for (int i = 0; i < nMFOG; i++)
            {
                WriteUInt(pos, 0);
                pos += 0x30;
            }
            return pos;
        }

        private int FixMODD(int pos)
        {
            WriteHeaderMagic(pos, "MODD");
            int size = ReadInt(pos + 4);
            int nMODD = size / 0x28;
            pos += 0x8;
            for (int i = 0; i < nMODD; i++)
            {
                Data[pos + 0x3] = 0;
                pos += 0x28;
            }

            return pos;
        }

        private int ReadMOMT(int pos)
        {
            RemoveUnwantedChunksUntil(pos, "MOMT");
            int momtSize = ReadInt(pos + 0x4);
            int nMOMT = momtSize / 0x40;
            pos += 0x8;

            for (int i = 0; i < nMOMT; i++)
            {
                int p = pos + 0x40 * i;

                var momtEntry = new MOMT();
                momtEntry.Flag1             = ReadUInt(p);
                momtEntry.Flag1             &= 0xFF;
                p += 4;
                momtEntry.ShaderType        = ReadUInt(p);

                switch (momtEntry.ShaderType)
                {
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 7: momtEntry.ShaderType = 6; break;
                    case 9: momtEntry.ShaderType = 0; break;
                    case 12: momtEntry.ShaderType = 5; break;
                    default:
                        if (momtEntry.ShaderType >= 13)
                            momtEntry.ShaderType = 4;
                        break;
                }

                p += 4;
                momtEntry.BlendMode         = ReadUInt(p);
                p += 4;

                momtEntry.TextureOffset1    = ReadUInt(p);
                var filename = AddFilename(momtEntry.TextureOffset1);
                if (filename != string.Empty)
                {
                    var idx = FilenamePosition.IndexOf(filename);
                    momtEntry.TextureOffset1 = (uint)idx;
                }
                else
                    momtEntry.TextureOffset1 = 0u;

                p += 4;

                momtEntry.SidnColor         = ReadUInt(p);
                p += 4;
                momtEntry.FrameSidnColor    = ReadUInt(p);
                p += 4;

                momtEntry.TextureOffset2    = ReadUInt(p);
                filename = AddFilename(momtEntry.TextureOffset2);
                if (filename != string.Empty)
                {
                    var idx = FilenamePosition.IndexOf(filename);
                    momtEntry.TextureOffset2 = (uint)idx;
                }
                else
                    momtEntry.TextureOffset2 = 0u;

                p += 4;

                momtEntry.DiffColor         = ReadUInt(p);
                p += 4;
                momtEntry.GroundType        = ReadUInt(p);
                p += 4;

                momtEntry.TextureOffset3    = ReadUInt(p);
                filename = AddFilename(momtEntry.TextureOffset3);
                if (filename != string.Empty)
                {
                    var idx = FilenamePosition.IndexOf(filename);
                    momtEntry.TextureOffset3 = (uint)idx;
                }
                else
                    momtEntry.TextureOffset3 = 0u;

                p += 4;

                momtEntry.Color             = ReadUInt(p);
                p += 4;
                momtEntry.Flag2             = ReadUInt(p);

                for (var j = 0; j < 4; ++j)
                {
                    p += 4;
                    momtEntry.RuntimeData[j] = ReadUInt(p);
                }

                MOMTEntries.Add(momtEntry);
            }

            return pos + momtSize;
        }

        private int WriteMOMT(int pos)
        {
            WriteHeaderMagic(pos, "MOMT");
            WriteInt(pos + 4, MOMTEntries.Count * 64);
            pos += 8;

            foreach (var momt in MOMTEntries)
            {
                WriteUInt(pos, momt.Flag1);
                WriteUInt(pos + 4, momt.ShaderType);
                WriteUInt(pos + 8, momt.BlendMode);
                WriteUInt(pos + 12, momt.TextureOffset1);
                WriteUInt(pos + 16, momt.SidnColor);
                WriteUInt(pos + 20, momt.FrameSidnColor);
                WriteUInt(pos + 24, momt.TextureOffset2);
                WriteUInt(pos + 28, momt.DiffColor);
                WriteUInt(pos + 32, momt.GroundType);
                WriteUInt(pos + 36, momt.TextureOffset3);
                WriteUInt(pos + 40, momt.Color);
                WriteUInt(pos + 44, momt.Flag2);

                WriteUInt(pos + 48, momt.RuntimeData[0]);
                WriteUInt(pos + 52, momt.RuntimeData[1]);
                WriteUInt(pos + 56, momt.RuntimeData[2]);
                WriteUInt(pos + 60, momt.RuntimeData[3]);
                pos += 64;
            }

            return pos;
        }

        private string AddFilename(uint fdid)
        {
            if (fdid != 0)
            {
                var textureFilename = Listfile.LookupFilename(fdid, ".wmo", System.IO.Path.GetFileName(wmoName));
                // Add the filename to the position list
                if (!FilenamePosition.Contains(textureFilename + "\0"))
                    FilenamePosition.Add(textureFilename + "\0");

                return textureFilename + "\0";
            }

            return string.Empty;
        }

        private int FixMOGI(int pos)
        {
            RemoveUnwantedChunksUntil(pos, "MOGI");
            int size = ReadInt(pos + 0x4);
            int nMOGI = size / 0x20;
            pos += 0x8;

            for (int i = 0; i < nMOGI; i++)
            {
                int p = pos + 0x20 * i;
                uint flag = ReadUInt(p);
                // remove 0x80 and high value flag
                flag &= 0x7FFFFF7F;
                WriteUInt(p, flag);
            }

            return pos + size;
        }

        private int SkipMCVP(int pos)
        {
            int mcvp = MagicToInt("MCVP");
            var chunks = ChunksOfs(pos, mcvp);

            if (chunks.ContainsKey(mcvp))
            {
                RemoveBytes(pos, chunks[mcvp] - pos);
                pos += ReadInt(pos + 0x4) + 0x8;
            }
            // remove the rest (new chunks, not handled by wotlk)
            RemoveBytes(pos, Size() - pos);

            return pos;
        }

        private int SkipChunk(int pos, string magic)
        {
            RemoveUnwantedChunksUntil(pos, magic);
            // Console.WriteLine($"Magic: {magic} Current Pos: {pos} New Pos: {pos + ReadInt(pos + 0x4) + 0x8}");
            return pos + ReadInt(pos + 0x4) + 0x8;
        }

        private uint CalculateTextureSize(List<string> filenameInput)
        {
            var textureSize = 0u;
            foreach (var texture in filenameInput)
                textureSize += (uint)texture.Length;

            return textureSize;
        }
    }

    public class MOMT
    {
        public uint Flag1;
        public uint ShaderType;
        public uint BlendMode;
        public uint TextureOffset1;
        public uint SidnColor;
        public uint FrameSidnColor;
        public uint TextureOffset2;
        public uint DiffColor;
        public uint GroundType;
        public uint TextureOffset3;
        public uint Color;
        public uint Flag2;
        public uint[] RuntimeData = new uint[4];
    }
}
