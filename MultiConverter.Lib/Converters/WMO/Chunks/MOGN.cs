using MultiConverter.Lib.Converters.Base;
using System.IO;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOGN : IChunk
    {
        public string Signature => "MOGN";
        public uint Length => (uint)Write().Length;
        public uint Order => 4;

        /// <summary>
        /// Group Names
        /// </summary>
        public byte[] GroupNames { get; set; }

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                GroupNames = reader.ReadBytes(inData.Length);
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(GroupNames, 0, GroupNames.Length);

                return stream.ToArray();
            }
        }
    }
}
