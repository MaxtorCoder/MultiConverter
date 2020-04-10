using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConverterLib
{
    public class ChunkedWowFile : WowFile
    {
        public ChunkedWowFile(string file) : base(file)
        {
        }

        public ChunkedWowFile(byte[] buff, int start, int count) : base("")
        {
            Data = new byte[count];
            Buffer.BlockCopy(buff, start, Data, 0, count);
        }

        public bool IsChunk(int pos, string magic)
        {
            return IsChunk(pos, MagicToInt(magic));
        }

        public bool IsChunk(int pos, int magic)
        {
            return BitConverter.ToInt32(Data, pos) == magic;
        }

        public void WriteHeaderMagic(int pos, string magic)
        {
            WriteInt(pos, MagicToInt(magic));
        }

        /// <summary>
        /// Remove all chunks between the starting position and the desired chunk
        /// </summary>
        /// <param name="start">starting pos</param>
        /// <param name="chunk">chunk</param>
        public void RemoveUnwantedChunksUntil(int start, string magic)
        {
            int value = MagicToInt(magic.ToUpper());
            // valid chunk
            if (value > 0)
            {
                RemoveUnwantedChunksUntil(start, value);
            }
        }

        /// <summary>
        /// Convert chunk magic to the value when stored in the file
        /// </summary>
        /// <param name="chunk">chunk magic (ex: MCNK)</param>
        /// <returns>chunkk magic</returns>
        public static int MagicToInt(string chunk)
        {
            if (chunk.Length != 4)
            {
                return 0;
            }

            int magic = 0;

            for (int i = 0; i < 4; ++i)
            {
                // it's inverted in the file
                magic += chunk[3 - i] << (8 * i);
            }

            return magic;
        }

        public static int InvertedMagicToInt(string chunk)
        {
            if (chunk.Length != 4)
            {
                return 0;
            }

            int magic = 0;

            for (int i = 0; i < 4; ++i)
            {
                // it's inverted in the file
                magic += chunk[i] << (8 * i);
            }

            return magic;
        }

        /// <summary>
        /// Remove all chunks between the starting position and the desired chunk
        /// </summary>
        /// <param name="start">starting pos</param>
        /// <param name="magic">chunk magic</param>
        public void RemoveUnwantedChunksUntil(int start, int magic)
        {
            if (Data.Length < start + 9)
            {
                return;
            }

            int header = ReadInt(start);
            int end = start;
            // check to prevent out of bound exception
            while (header != magic && Data.Length > end + 8)
            {
                end += ReadInt(end + 0x4) + 0x8;

                if (Data.Length < end + 4)
                {
                    break;
                }
                header = ReadInt(end);
            }
            RemoveBytes(start, end - start);
        }

        public Dictionary<int, int> ChunksOfs(int start, string end_magic)
        {
            return ChunksOfs(start, MagicToInt(end_magic));
        }
        public Dictionary<int, int> ChunksOfs(int start, int end_magic)
        {
            Dictionary<int, int> chunk_pos = new Dictionary<int, int>();

            if (start < Data.Length)
            {
                int pos = start;
                int magic = ReadInt(pos);

                if (magic == end_magic)
                    chunk_pos.Add(magic, pos);

                while (magic != end_magic && pos <= Data.Length - 8)
                {
                    magic = ReadInt(pos);
                    chunk_pos.Add(magic, pos);

                    int size = ReadInt(pos + 0x4) + 0x8;
                    pos += size;
                }
            }

            return chunk_pos;
        }
    }
}
