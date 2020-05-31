using MultiConverter.Lib.Readers.Base;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOVV : ChunkBase
    {
        public override string Signature => "MOVV";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 10;

        public override void Read(byte[] inData) { }

        public override byte[] Write() => new byte[0];
    }
}
