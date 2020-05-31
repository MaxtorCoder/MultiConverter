using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOGI : ChunkBase
    {
        public override string Signature => "MOGI";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 5;

        /// <summary>
        /// List of <see cref="MOGIEntry"/>
        /// </summary>
        public List<MOGIEntry> MOGIs = new List<MOGIEntry>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var mogiSize = inData.Length / 32;
                for (var i = 0; i < mogiSize; ++i)
                {
                    var mogi = new MOGIEntry();
                    mogi.Flags = reader.ReadUInt32();

                    for (var j = 0; j < 3; ++j)
                        mogi.BBoxCorner1[j] = reader.ReadSingle();

                    for (var j = 0; j < 3; ++j)
                        mogi.BBoxCorner2[j] = reader.ReadSingle();

                    mogi.NameOffset = reader.ReadInt32();

                    MOGIs.Add(mogi);
                }
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mogi in MOGIs)
                {
                    mogi.Flags &= 0x7FFFFF7F;
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
