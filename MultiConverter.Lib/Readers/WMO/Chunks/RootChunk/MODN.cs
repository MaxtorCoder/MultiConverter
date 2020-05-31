using MultiConverter.Lib.Readers.Base;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MODN : ChunkBase
    {
        public override string Signature => "MODN";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 14;

        /// <summary>
        /// List of DoodadFilenames.
        /// </summary>
        public List<string> DoodadFilenames = new List<string>();

        public override void Read(byte[] inData) { }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // O(n^2), really don't care at this point.
                foreach (var filename in DoodadFilenames)
                    foreach (var fileChar in filename)
                        writer.Write(fileChar);

                return stream.ToArray();
            }
        }
    }
}
