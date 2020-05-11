using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOPV : IChunk
    {
        public string Signature => "MOPV";
        public uint Length => (uint)Write().Length;
        public uint Order => 7;

        /// <summary>
        /// List of <see cref="MOPVEntry"/>.
        /// </summary>
        public List<MOPVEntry> MOPVs = new List<MOPVEntry>();

        public void Read(byte[] inData)
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

        public byte[] Write()
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
