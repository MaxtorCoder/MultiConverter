using MultiConverter.Lib.Converters.Base;
using System;
using System.IO;

namespace MultiConverter.Lib.Converters
{
    public class WDTConverter : ChunkedWowFile, IConverter
    {
        public WDTConverter(string file) : base(file)
        {
        }

        public bool Fix()
        {
            int flags = ReadInt(0x14);
            if ((flags & 0x80) != 0)
            {
                flags |= 0x4;
            }
            Data[0x14] = (byte)(flags & 0x1f);

            for (int i = 0x3C; i < 0x3C + 32768; i += 0x8)
            {
                Data[i] &= 0x1;
            }

            return true;
        }
    }
}
