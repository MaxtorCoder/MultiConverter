using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Converters.WMO.Entries
{
    public class MOPTEntry
    {
        public ushort StartVertex { get; set; }
        public ushort Count { get; set; }
        public C4Plane Plane { get; set; }
    }
}
