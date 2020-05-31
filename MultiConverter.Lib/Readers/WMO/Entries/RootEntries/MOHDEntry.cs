using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Readers.WMO.Entries
{
    public class MOHDEntry
    {
        public uint MaterialCount { get; set; }
        public uint GroupCount { get; set; }
        public uint PortalCount { get; set; }
        public uint LightCount { get; set; }
        public uint ModelCount { get; set; }
        public uint DoodadCount { get; set; }
        public uint SetCount { get; set; }
        public CArgb Color { get; set; }
        public uint WMOId { get; set; }
        public float[] BBoxCorner1 { get; set; } = new float[3];
        public float[] BBoxCorner2 { get; set; } = new float[3];
        public ushort FlagLod { get; set; }
        public ushort LodCount { get; set; }
    }
}
