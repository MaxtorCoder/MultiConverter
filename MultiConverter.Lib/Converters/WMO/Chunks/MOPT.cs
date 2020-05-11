using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using MultiConverter.Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOPT : IChunk
    {
        public string Signature => "MOPT";
        public uint Length => (uint)Write().Length;
        public uint Order => 8;

        /// <summary>
        /// A list of <see cref="MOPTEntry"/>.
        /// </summary>
        public List<MOPTEntry> MOPTs = new List<MOPTEntry>();

        public void Read(byte[] inData)
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

        public byte[] Write()
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
