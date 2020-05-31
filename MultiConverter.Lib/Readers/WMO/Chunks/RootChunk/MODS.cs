using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using System.Collections.Generic;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MODS : ChunkBase
    {
        public override string Signature => "MODS";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 13;

        /// <summary>
        /// List of <see cref="MODSEntry"/>
        /// </summary>
        public List<MODSEntry> MODSs = new List<MODSEntry>();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var modsSize = inData.Length / 32;
                for (var i = 0; i < modsSize; ++i)
                {
                    MODSs.Add(new MODSEntry
                    {
                        SetName = new string(reader.ReadChars(20)),
                        FirstDoodadInSet = reader.ReadUInt32(),
                        DoodadInSetCount = reader.ReadUInt32(),
                        Padding = reader.ReadUInt32()
                    });
                }
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var mods in MODSs)
                {
                    for (var i = 0; i < 20; ++i)
                        writer.Write(mods.SetName[i]);

                    writer.Write(mods.FirstDoodadInSet);
                    writer.Write(mods.DoodadInSetCount);
                    writer.Write(mods.Padding);
                }

                return stream.ToArray();
            }
        }
    }
}
