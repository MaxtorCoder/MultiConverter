namespace MultiConverter.Lib.Converters.WMO.Entries
{
    public class MOGIEntry
    {
        public uint Flags { get; set; }
        public float[] BBoxCorner1 { get; set; } = new float[3];
        public float[] BBoxCorner2 { get; set; } = new float[3];
        public int NameOffset { get; set; }
    }
}
