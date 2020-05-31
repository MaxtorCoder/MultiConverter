using MultiConverter.Lib.Readers.Base;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOGN : ChunkBase
    {
        public override string Signature => "MOGN";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 4;

        /// <summary>
        /// Group Names
        /// </summary>
        public byte[] GroupNames { get; set; }

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                GroupNames = reader.ReadBytes(inData.Length);
            }
        }

        public override byte[] Write()
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
