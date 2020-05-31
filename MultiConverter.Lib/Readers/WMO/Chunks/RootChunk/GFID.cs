using MultiConverter.Lib.Readers.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class GFID : ChunkBase
    {
        public override string Signature => "GFID";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 0;

        public List<uint> GroupFileDataIds = new List<uint>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var gfidSize = inData.Length / 4;
                for (var i = 0; i < gfidSize; ++i)
                    GroupFileDataIds.Add(reader.ReadUInt32());
            }
        }

        public override byte[] Write()
        {
            throw new NotImplementedException();
        }
    }
}
