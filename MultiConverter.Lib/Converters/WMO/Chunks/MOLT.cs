using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using MultiConverter.Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOLT : IChunk
    {
        public string Signature => "MOLT";
        public uint Length => (uint)Write().Length;
        public uint Order => 12;

        /// <summary>
        /// List of <see cref="MOLTEntry"/>.
        /// </summary>
        public List<MOLTEntry> MOLTs = new List<MOLTEntry>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var moltSize = inData.Length / 48;
                for (var i = 0; i < moltSize; ++i)
                {
                    var molt = new MOLTEntry();
                    molt.Type = reader.ReadByte();

                    for (var j = 0; j < 3; ++j)
                        molt.Flags[j] = reader.ReadByte();

                    molt.Color = reader.ReadCArgb();
                    molt.Position = reader.ReadC3Vector();
                    molt.Intensity = reader.ReadSingle();

                    for (var j = 0; j < 4; ++j)
                        molt.UnkShit[j] = reader.ReadSingle();

                    molt.AttenStart = reader.ReadSingle();
                    molt.AttenEnd = reader.ReadSingle();
                }
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var molt in MOLTs)
                {
                    writer.Write(molt.Type);
                    writer.Write(molt.Flags);
                    writer.WriteCArgb(molt.Color);
                    writer.WriteC3Vector(molt.Position);
                    writer.Write(molt.Intensity);

                    for (var j = 0; j < 4; ++j)
                        writer.Write(molt.UnkShit[j]);

                    writer.Write(molt.AttenStart);
                    writer.Write(molt.AttenEnd);
                }

                return stream.ToArray();
            }
        }
    }
}
