using MultiConverter.Lib.Converters.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOTX : IChunk
    {
        public string Signature => "MOTX";
        public uint Length => (uint)Write().Length;
        public uint Order => 2;

        public List<string> Filenames = new List<string>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                throw new Exception("I am pleb, come back later");
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // O(n^2), really don't care at this point.
                foreach (var filename in Filenames)
                    foreach (var fileChar in filename)
                        writer.Write(fileChar);

                return stream.ToArray();
            }
        }
    }
}
