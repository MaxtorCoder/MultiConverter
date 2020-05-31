using MultiConverter.Lib.Readers.Base;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOSB : ChunkBase
    {
        public override string Signature => "MOSB";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 6;

        /// <summary>
        /// Originaly the skybox filename.
        /// </summary>
        public uint Padding { get; set; }

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                Padding = reader.ReadUInt32();
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Padding);

                return stream.ToArray();
            }
        }
    }
}
