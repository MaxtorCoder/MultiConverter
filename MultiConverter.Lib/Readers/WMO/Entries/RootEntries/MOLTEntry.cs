using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Readers.WMO.Entries
{
    public class MOLTEntry
    {
        public byte Type { get; set; }
        public byte[] Flags { get; set; } = new byte[3];
        public CArgb Color { get; set; }
        public C3Vector Position { get; set; }
        public float Intensity { get; set; }
        public float[] UnkShit { get; set; } = new float[4];
        public float AttenStart { get; set; }
        public float AttenEnd { get; set; }
    }
}
