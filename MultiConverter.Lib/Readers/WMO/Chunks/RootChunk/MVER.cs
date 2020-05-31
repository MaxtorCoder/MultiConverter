using MultiConverter.Lib.Readers.Base;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MVER : ChunkBase
    {
        public override string Signature => "MVER";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 0;

        /// <summary>
        /// The Version of <see cref="WMOFile"/>.
        /// </summary>
        public uint Version { get; set; }

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                Version = reader.ReadUInt32();
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Version);

                return stream.ToArray();
            }
        }
    }
}
