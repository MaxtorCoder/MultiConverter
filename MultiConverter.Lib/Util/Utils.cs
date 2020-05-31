using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiConverter.Lib
{
    public static class Utils
    {
        public static bool IsCorrectFile(string s)
        {
            return s.EndsWith("m2", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith("anim", StringComparison.OrdinalIgnoreCase)
                || s.EndsWith("wmo", StringComparison.OrdinalIgnoreCase)
                || (s.EndsWith("wdt", StringComparison.OrdinalIgnoreCase)
                  // todo: match with a regex maybe ?
                  && !(s.EndsWith("_lgt.wdt", StringComparison.OrdinalIgnoreCase))
                  && !(s.EndsWith("_occ.wdt", StringComparison.OrdinalIgnoreCase))
                  && !(s.EndsWith("_fog.wdt", StringComparison.OrdinalIgnoreCase))
                   )
                || (Regex.IsMatch(s.ToLower(), @".*((_[0-9]{1,2}){2})(\.adt)"))
                ;
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

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Read the .build.info from the local storage and returns the branch list
        /// </summary>
        /// <param name="filepath">The filepath to .build.info</param>
        /// <returns>Returns the branc list.</returns>
        public static List<string> GetLocalBranch(string filepath)
        {
            var branchList = new List<string>();

            using (var reader = new StreamReader(filepath))
            {
                // Read 2 useless files.
                reader.ReadLine();
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineSplit = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    var branch = lineSplit.Last();
                    if (!branchList.Contains(branch))
                        branchList.Add(branch);
                }
            }

            return branchList;
        }
    }
}
