using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using MultiConverter.Lib.Util;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOLT : ChunkBase
    {
        public override string Signature => "MOLT";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 12;

        /// <summary>
        /// List of <see cref="MOLTEntry"/>.
        /// </summary>
        public List<MOLTEntry> MOLTs = new List<MOLTEntry>();

        public override void Read(byte[] inData)
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

        public override byte[] Write()
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
