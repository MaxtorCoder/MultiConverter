using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOGI : IChunk
    {
        public string Signature => "MOGI";
        public uint Length => (uint)Write().Length;
        public uint Order => 5;

        /// <summary>
        /// List of <see cref="MOGIEntry"/>
        /// </summary>
        public List<MOGIEntry> MOGIs = new List<MOGIEntry>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var mogiSize = inData.Length / 32;
                for (var i = 0; i < mogiSize; ++i)
                {
                    var mogi = new MOGIEntry();
                    mogi.Flags = reader.ReadUInt32();
                    mogi.Flags &= 0x7FFFFF7F;

                    for (var j = 0; j < 3; ++j)
                        mogi.BBoxCorner1[j] = reader.ReadSingle();

                    for (var j = 0; j < 3; ++j)
                        mogi.BBoxCorner2[j] = reader.ReadSingle();

                    mogi.NameOffset = reader.ReadInt32();

                    MOGIs.Add(mogi);
                }
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mogi in MOGIs)
                {
                    writer.Write(mogi.Flags);

                    for (var i = 0; i < 3; ++i)
                        writer.Write(mogi.BBoxCorner1[i]);

                    for (var i = 0; i < 3; ++i)
                        writer.Write(mogi.BBoxCorner2[i]);

                    writer.Write(mogi.NameOffset);
                }

                return stream.ToArray();
            }
        }
    }
}
