using MultiConverter.Lib.Readers.Base;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOVB : ChunkBase
    {
        public override string Signature => "MOVB";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 11;

        public override void Read(byte[] inData) { }

        public override byte[] Write() => new byte[0];
    }
}
