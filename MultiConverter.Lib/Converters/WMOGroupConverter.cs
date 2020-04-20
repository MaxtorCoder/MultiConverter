using MultiConverter.Lib.Converters.Base;
using System;
using System.IO;

namespace MultiConverter.Lib.Converters
{
    public enum GroupChunk
    {
        MOGP = 1297041232,
        MOPY = 1297043545,
        MOVI = 1297045065,
        MOVT = 1297045076,
        MONR = 1297043026,
        MOTV = 1297044566,
        MOBA = 1297039937,
        MOLR = 1297042514,
        MODR = 1297040466,
        MOBN = 1297039950,
        MOBR = 1297039954,
        MOCR = 1297039954,
        MOCV = 1297040214,
        MLIQ = 1296845137
    };

    public class WMOGroupConverter : ChunkedWowFile, IConverter
    {
        private int posMOVT = 0;
        private bool wod;

        public WMOGroupConverter(string wmo, bool wod = false) : base(wmo)
        {
            this.wod = wod;
        }

        private bool FixWod()
        {
            int pos_moba = CheckChunk(0xC, GroupChunk.MOTV, false, false);
            FixMOBA(pos_moba);

            return true;
        }

        private bool FixLK()
        {
            int pos = 0xC, posMOBA = 0, posMLIQ = 0;

            uint flags = ReadUInt(0x1C);
            WriteUInt(0x1C, flags & 0x7FFFFFF);

            // MOPY pos
            pos = 0x58;

            // Mandatory chunks

            pos = CheckChunk(pos, GroupChunk.MOPY);
            pos = CheckChunk(pos, GroupChunk.MOVI); posMOVT = pos;
            pos = CheckChunk(pos, GroupChunk.MOVT);
            pos = CheckChunk(pos, GroupChunk.MONR);
            pos = CheckChunk(pos, GroupChunk.MOTV); posMOBA = pos;
            pos = CheckChunk(pos, GroupChunk.MOBA);

            // Optionnal chunks
            if ((flags & 0x200) != 0)
            {
                pos = CheckChunk(pos, GroupChunk.MOLR);
            }
            if ((flags & 0x800) != 0)
            {
                pos = CheckChunk(pos, GroupChunk.MODR);
            }
            if ((flags & 0x1) != 0)
            {
                int pos_mobn = FindChunk(pos, GroupChunk.MOBN);
                int pos_mobr = FindChunk(pos, GroupChunk.MOBR);

                if (pos_mobn == -1 && pos_mobr == -1)
                {
                    // remove flag (fix client freezing)
                    WriteInt(0x1C, (int)flags & ~0x1);
                }
                else
                {
                    pos = CheckChunk(pos, GroupChunk.MOBN);
                    pos = CheckChunk(pos, GroupChunk.MOBR);
                }
            }
            if ((flags & 0x4) != 0)
            {
                pos = CheckChunk(pos, GroupChunk.MOCV);
            }
            if ((flags & 0x1000) != 0)
            {
                posMLIQ = pos;
                pos = CheckChunk(pos, GroupChunk.MLIQ);
            }
            if ((flags & 0x2000000) != 0)
            {
                pos = CheckChunk(pos, GroupChunk.MOTV);
            }
            if ((flags & 0x1000000) != 0)
            {
                pos = CheckChunk(pos, GroupChunk.MOCV);
            }

            // Cleanup
            RemoveBytes(pos, Size() - pos);

            FixMOBA(posMOBA);

            int liquidID = ReadInt(0x48);
            if (liquidID > 181)
            {
                WriteInt(0x48, 5);
            }

            return true;
        }

        public bool Fix()
        {
            if (!Valid || Size() < 0x58 || !IsChunk(0xC, "MOGP"))
            {
                return false;
            }

            return wod ? FixWod() : FixLK();
        }

        private void FixMOBA(int pos)
        {
            int size = ReadInt(pos + 0x4);
            int n = size / 0x18;
            pos += 0x8;



            for (int i = 0; i < n; i++)
            {
                if ((Data[pos + 0x16] & 2) != 0)
                {
                    Data[pos + 0x16] = 0;
                    Data[pos + 0x17] = Data[pos + 0xA];
                    FixMOBA_box(pos, ReadUShort(pos + 0x12), ReadUShort(pos + 0x14));
                }
                pos += 0x18;
            }
        }

        private void GetVerticeCoord(ushort id, out float x, out float y, out float z)
        {
            int pos = posMOVT + 0x8 + 0xC * id;

            x = ReadFloat(pos);
            y = ReadFloat(pos + 0x4);
            z = ReadFloat(pos + 0x8);
        }

        private void FixMOBA_box(int box, ushort start, ushort end)
        {
            float[] f = { float.MaxValue, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue, float.MinValue };

            for (ushort i = start; i <= end; i++)
            {
                for (int k = 0; k < 3; ++k)
                {
                    float x, y, z;

                    GetVerticeCoord(i, out x, out y, out z);

                    f[0] = Math.Min(x, f[0]);
                    f[3] = Math.Max(x, f[3]);

                    f[1] = Math.Min(y, f[1]);
                    f[4] = Math.Max(y, f[4]);

                    f[2] = Math.Min(z, f[2]);
                    f[5] = Math.Max(z, f[5]);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                WriteShort(box + 2 * i, (short)((i < 3 ? Math.Floor(f[i]) : Math.Ceiling(f[i]))));
            }
        }

        private int FindChunk(int pos, GroupChunk chunk)
        {
            int found = -1;
            for (int i = pos; i + 8 <= Size(); i += (ReadInt(i + 0x4) + 0x8))
            {
                if (ReadInt(i) == (int)chunk)
                {
                    found = i;
                    break;
                }
            }

            return found;
        }


        /// <summary>
        /// Check if the chunk is present, if not it'll be created with a size of 0 if create is true
        /// Additionnal chunks between the starting pos and the chunk will be deleted
        /// </summary>
        /// <param name="pos">starting pos for the search</param>
        /// <param name="chunk">magic value of the wanted chunk</param>
        /// <param name="create">if this is set to true and the chunk isn't there it will create it empty</param>
        /// <returns>the new pos after the chunk</returns>
        private int CheckChunk(int pos, GroupChunk chunk, bool create = true, bool delete = true)
        {
            int found = FindChunk(pos, chunk);

            if (found == -1 && create)
            {
                AddEmptyBytes(pos, 8);
                WriteInt(pos, (int)chunk);
                return pos + 0x8;
            }
            if (found > pos)
            {
                if (delete)
                {
                    RemoveBytes(pos, found - pos);
                }
                else
                {
                    pos = found;
                }
            }

            if (found != -1)
            {
                pos += ReadInt(pos + 0x4) + 0x8;
            }

            // return the current pos
            return pos;
        }
    }
}
