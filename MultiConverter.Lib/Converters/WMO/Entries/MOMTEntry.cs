using MultiConverter.Lib.Common;

namespace MultiConverter.Lib.Converters.WMO.Entries
{
    public class MOMTEntry
    {
        public uint Flags1 { get; set; }
        public uint ShaderType { get; set; }
        public uint BlendMode { get; set; }
        public uint TextureId1 { get; set; }
        public CArgb SidnColor { get; set; }
        public CArgb FrameSidnColor { get; set; }
        public uint TextureId2 { get; set; }
        public CArgb DiffColor { get; set; }
        public uint GroundType { get; set; }
        public uint TextureId3 { get; set; }
        public CArgb Color { get; set; }
        public uint Flags2 { get; set; }
        public uint[] RunTimeData { get; set; } = new uint[4];
    }
}
