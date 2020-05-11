using MultiConverter.Lib.Converters.Base;
using System.IO;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MVER : IChunk
    {
        public string Signature => "MVER";
        public uint Length => (uint)Write().Length;
        public uint Order => 0;

        /// <summary>
        /// The Version of <see cref="WMOFile"/>.
        /// </summary>
        public uint Version { get; set; }

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                Version = reader.ReadUInt32();
            }
        }

        public byte[] Write()
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
