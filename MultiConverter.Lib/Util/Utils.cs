using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MultiConverter.Lib
{
    public class Utils
    {
        public static bool IsCorrectFile(string s)
        {
            return s.EndsWith(".m2", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".anim", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".adt", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".wdt", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith(".skin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsChunk(ref byte[] buff, int pos, int magic)
        {
            return BitConverter.ToInt32(buff, pos) == magic;
        }

        public static void CopyInt(ref byte[] buff, int pos, int value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, buff, pos, 4);
        }

        public static void RemoveBytes(ref byte[] buff, int start, int count)
        {
            int size = buff.Length;

            if (size <= start + count || count <= 0)
            {
                return;
            }

            byte[] tmp = new byte[size - count];
            if (start > 0)
            {
                Buffer.BlockCopy(buff, 0, tmp, 0, start);
            }
            Buffer.BlockCopy(buff, start + count, tmp, start, size - count - start);
            buff = tmp;
        }

        public static void AddEmptyBytes(ref byte[] buff, int start, int count)
        {
            int size = buff.Length;
            byte[] tmp = new byte[size + count];
            Buffer.BlockCopy(buff, 0, tmp, 0, start);
            Buffer.BlockCopy(buff, start, tmp, start + count, size - start);
            buff = tmp;
        }

        public static void InsertBytes(ref byte[] buff, int start, byte[] bytes)
        {
            AddEmptyBytes(ref buff, start, bytes.Length);
            Buffer.BlockCopy(bytes, 0, buff, start, bytes.Length);
        }

        public static void RemoveUnwantedChunks(ref byte[] tab, int start, int magic)
        {
            if (tab.Length < start + 9)
            {
                return;
            }

            int header = BitConverter.ToInt32(tab, start);
            int end = start;
            // check to prevent out of bound exception
            while (header != magic && tab.Length > end + 8)
            {
                if (header < 1291845632)
                {
                    end += 0x4;
                }
                else
                {
                    end += BitConverter.ToInt32(tab, start + 0x4) + 0x8;
                }
                if (tab.Length < end + 4)
                {
                    break;
                }
                header = BitConverter.ToInt32(tab, end);
            }
            RemoveBytes(ref tab, start, end - start);
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
