using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Readers.WMO.Entries
{
    public class MFOGEntry
    {
        public uint Flags { get; set; }
        public C3Vector Position { get; set; }
        public float SmallRadius { get; set; }
        public float LargeRadius { get; set; }
        public float FogEnd { get; set; }
        public float FogStartMultiplier { get; set; }
        public CArgb FogColor { get; set; }
        public float Unk1 { get; set; }
        public float Unk2 { get; set; }
        public CArgb FogColor2 { get; set; }
    }
}
