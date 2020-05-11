using MultiConverter.Lib.Converters.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOSB : IChunk
    {
        public string Signature => "MOSB";
        public uint Length => (uint)Write().Length;
        public uint Order => 6;

        /// <summary>
        /// Originaly the skybox filename.
        /// </summary>
        public uint Padding { get; set; }

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                Padding = reader.ReadUInt32();
            }
        }

        public byte[] Write()
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
