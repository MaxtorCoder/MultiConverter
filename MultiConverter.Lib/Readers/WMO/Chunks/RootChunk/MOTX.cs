using MultiConverter.Lib.Readers.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOTX : ChunkBase
    {
        public override string Signature => "MOTX";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 2;

        public List<string> Filenames = new List<string>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                throw new Exception("I am pleb, come back later");
            }
        }

        public override byte[] Write()
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
