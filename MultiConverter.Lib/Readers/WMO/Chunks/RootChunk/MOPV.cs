using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOPV : ChunkBase
    {
        public override string Signature => "MOPV";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 7;

        /// <summary>
        /// List of <see cref="MOPVEntry"/>.
        /// </summary>
        public List<MOPVEntry> MOPVs = new List<MOPVEntry>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var mopvSize = inData.Length / 48;
                for (var i = 0; i < mopvSize; ++i)
                {
                    var mopv = new MOPVEntry();

                    for (var j = 0; j < 3; ++j)
                        mopv.Corner1[j] = reader.ReadSingle();

                    for (var j = 0; j < 3; ++j)
                        mopv.Corner2[j] = reader.ReadSingle();

                    for (var j = 0; j < 3; ++j)
                        mopv.Corner3[j] = reader.ReadSingle();

                    for (var j = 0; j < 3; ++j)
                        mopv.Corner4[j] = reader.ReadSingle();

                    MOPVs.Add(mopv);
                }
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mopv in MOPVs)
                {
                    for (var j = 0; j < 3; ++j)
                        writer.Write(mopv.Corner1[j]);

                    for (var j = 0; j < 3; ++j)
                        writer.Write(mopv.Corner2[j]);

                    for (var j = 0; j < 3; ++j)
                        writer.Write(mopv.Corner3[j]);

                    for (var j = 0; j < 3; ++j)
                        writer.Write(mopv.Corner4[j]);
                }

                return stream.ToArray();
            }
        }
    }
}
