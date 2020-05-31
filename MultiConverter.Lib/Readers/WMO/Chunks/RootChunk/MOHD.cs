using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using MultiConverter.Lib.Util;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MOHD : ChunkBase
    {
        public override string Signature => "MOHD";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 1;

        public MOHDEntry MOHDEntry { get; set; } = new MOHDEntry();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                MOHDEntry.MaterialCount = reader.ReadUInt32();
                MOHDEntry.GroupCount    = reader.ReadUInt32();
                MOHDEntry.PortalCount   = reader.ReadUInt32();
                MOHDEntry.LightCount    = reader.ReadUInt32();
                MOHDEntry.ModelCount    = reader.ReadUInt32();
                MOHDEntry.DoodadCount   = reader.ReadUInt32();
                MOHDEntry.SetCount      = reader.ReadUInt32();

                MOHDEntry.Color         = reader.ReadCArgb();
                MOHDEntry.WMOId         = reader.ReadUInt32();

                for (var i = 0; i < 3; ++i)
                    MOHDEntry.BBoxCorner1[i] = reader.ReadSingle();

                for (var i = 0; i < 3; ++i)
                    MOHDEntry.BBoxCorner2[i] = reader.ReadSingle();

                MOHDEntry.FlagLod       = reader.ReadUInt16();
                MOHDEntry.LodCount      = reader.ReadUInt16();
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(MOHDEntry.MaterialCount);
                writer.Write(MOHDEntry.GroupCount);
                writer.Write(MOHDEntry.PortalCount);
                writer.Write(MOHDEntry.LightCount);
                writer.Write(MOHDEntry.ModelCount);
                writer.Write(MOHDEntry.DoodadCount);
                writer.Write(MOHDEntry.SetCount);

                writer.WriteCArgb(MOHDEntry.Color);
                writer.Write(MOHDEntry.WMOId);

                for (var i = 0; i < 3; ++i)
                    writer.Write(MOHDEntry.BBoxCorner1[i]);

                for (var i = 0; i < 3; ++i)
                    writer.Write(MOHDEntry.BBoxCorner2[i]);

                writer.Write(MOHDEntry.FlagLod);
                writer.Write(MOHDEntry.LodCount);

                return stream.ToArray();
            }
        }
    }
}
