using System;
using System.IO;

namespace MultiConverterLib
{
    public class WMORootConverter : ChunkedWowFile, IConverter
    {

        public WMORootConverter(string wmo) : base(wmo)
        {
        }

        public bool Fix()
        {
            if (!Valid || Size() < 0x5C)
                return false;

            int pos = 0;// 0x54 + ReadInt(0x58) + 0x8;

            pos = SkipChunk(pos, "MVER");
            pos = SkipChunk(pos, "MOHD");
            pos = SkipChunk(pos, "MOTX");
            pos = FixMOMT(pos);// MOMT
            pos = SkipChunk(pos, "MOGN");
            pos = FixMOGI(pos);// MOGI

            int mosb = MagicToInt("MOSB");
            // todo: use this for all chunks !
            var ofs = ChunksOfs(pos, mosb);

            if (!ofs.ContainsKey(mosb))
            {
                AddEmptyBytes(pos, 0xC);
                WriteInt(pos, mosb);
                WriteInt(pos + 0x4, 4);
                pos += 0xC;
            }
            else
            {
                pos = SkipChunk(pos, "MOSB");
            }

            pos = SkipChunk(pos, "MOPV");
            pos = SkipChunk(pos, "MOPT");
            pos = SkipChunk(pos, "MOPR");
            pos = SkipChunk(pos, "MOVV");
            pos = SkipChunk(pos, "MOVB");

            int pos_molt = pos;
            pos = SkipChunk(pos, "MOLT");
            // fix nLights
            WriteInt(0x20, ReadInt(pos_molt + 0x4) / 0x30);

            pos = SkipChunk(pos, "MODS");
            pos = SkipChunk(pos, "MODN");
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
            RemoveUnwantedChunksUntil(pos, "MODD");
            int size = ReadInt(pos + 0x4);
            int nMODD = size / 0x28;
            pos += 0x8;
            for (int i = 0; i < nMODD; i++)
            {
                Data[pos + 0x3] = 0;
                pos += 0x28;
            }

            return pos;
        }

        private int FixMOMT(int pos)
        {
            RemoveUnwantedChunksUntil(pos, "MOMT");
            int momtSize = ReadInt(pos + 0x4);
            int nMOMT = momtSize / 0x40;
            pos += 0x8;

            for (int i = 0; i < nMOMT; i++)
            {
                int p = pos + 0x40 * i;
                uint flag = ReadUInt(p);
                flag &= 0xFF;
                WriteUInt(p, flag);

                p += 0x4;
                uint shader = ReadUInt(p);

                switch (shader)
                {
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 7: shader = 6; break;
                    case 9: shader = 0; break;
                    case 12: shader = 5; break;
                    /*case 13:// flag = 4; break;

                     */
                    default:
                        if (shader >= 13)
                            shader = 4;
                        break;
                }

                WriteUInt(p, shader);
            }

            return pos + momtSize;
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
            return pos + ReadInt(pos + 0x4) + 0x8;
        }
    }
}
