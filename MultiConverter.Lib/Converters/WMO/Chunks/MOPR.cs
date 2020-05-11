using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOPR : IChunk
    {
        public string Signature => "MOPR";
        public uint Length => (uint)Write().Length;
        public uint Order => 9;

        /// <summary>
        /// List of <see cref="MOPREntry"/>.
        /// </summary>
        public List<MOPREntry> MOPRs = new List<MOPREntry>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var moprSize = inData.Length / 8;
                for (var i = 0; i < moprSize; ++i)
                {
                    MOPRs.Add(new MOPREntry
                    {
                        PortalIndex = reader.ReadUInt16(),
                        WMOGroupIndex = reader.ReadUInt16(),
                        Direction = reader.ReadInt16(),
                        AlwaysZero = reader.ReadUInt16(),
                    });
                }
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mopr in MOPRs)
                {
                    writer.Write(mopr.PortalIndex);
                    writer.Write(mopr.WMOGroupIndex);
                    writer.Write(mopr.Direction);
                    writer.Write(mopr.AlwaysZero);
                }

                return stream.ToArray();
            }
        }
    }
}
