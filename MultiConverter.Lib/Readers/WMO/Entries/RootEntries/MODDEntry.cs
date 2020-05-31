using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Readers.WMO.Entries
{
    public class MODDEntry
    {
        public uint NameIndex { get; set; }
        public byte Flags { get; set; }
        public C3Vector Position { get; set; }
        public C3Vector Rotation { get; set; }
        public float RotationW { get; set; }
        public float Scale { get; set; }
        public CArgb Color { get; set; }
    }
}
