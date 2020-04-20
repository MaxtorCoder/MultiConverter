using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConverterLib
{
    public class AnimConverter : ChunkedWowFile, IConverter
    {
        public AnimConverter(string file) : base(file)
        {

        }

        public bool Fix()
        {
            if (IsChunk(0, "2MFA"))
            {
                RemoveBytes(0, 0x8);
                return true;
            }

            return false;
        }
    }
}
