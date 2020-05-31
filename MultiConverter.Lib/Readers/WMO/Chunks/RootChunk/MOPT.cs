using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using MultiConverter.Lib.Util;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOPT : ChunkBase
    {
        public override string Signature => "MOPT";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 8;

        /// <summary>
        /// A list of <see cref="MOPTEntry"/>.
        /// </summary>
        public List<MOPTEntry> MOPTs = new List<MOPTEntry>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var moptSize = inData.Length / 20;
                for (var i = 0; i < moptSize; ++i)
                {
                    var mopt = new MOPTEntry
                    {
                        StartVertex = reader.ReadUInt16(),
                        Count       = reader.ReadUInt16(),
                        Plane       = reader.ReadC4Plane()
                    };

                    MOPTs.Add(mopt);
                }
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mopt in MOPTs)
                {
                    writer.Write(mopt.StartVertex);
                    writer.Write(mopt.Count);
                    writer.WriteC4Plane(mopt.Plane);
                }

                return stream.ToArray();
            }
        }
    }
}
