using MultiConverter.Lib.Converters.Base;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MODN : IChunk
    {
        public string Signature => "MODN";
        public uint Length => (uint)Write().Length;
        public uint Order => 14;

        /// <summary>
        /// List of DoodadFilenames.
        /// </summary>
        public List<string> DoodadFilenames = new List<string>();

        public void Read(byte[] inData) { }

        public byte[] Write()
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
