using MultiConverter.Lib.Readers.Base;

namespace MultiConverter.Lib.Readers
{
    public class AnimConverter : ChunkedWowFile
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
