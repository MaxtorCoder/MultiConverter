using MultiConverter.Lib.Common;
using MultiConverter.Lib.Constants;
using MultiConverter.Lib.Util;
using System.IO;

namespace MultiConverter.Lib.Readers.WMO.Entries.GroupEntries
{
    public class MOGPEntry
    {
        public uint NameOffset;
        public uint DescriptionNameOffset;
        public MOGPFlags Flags;
        public C3Vector BoundingBox1;
        public C3Vector BoundingBox2;
        public ushort MOPRIndex;
        public ushort MOPRCount;
        public ushort NumBatchesA;
        public ushort NumBatchesB;
        public uint NumBatchesC;
        public byte[] FogIndices = new byte[4];
        public uint LiquidType;
        public uint WMOGroupId;
        public uint Unk1;
        public uint Unk2;

        public MOVIEntry[] Indices;
        public MOPYEntry[] MaterialInfo;
        public MOVTEntry[] Vertices;
        public MOTVEntry[][] TextureCoords;
        public MONREntry[] Normals;
        public MOBAEntry[] RenderBatches;

        public void Read(BinaryReader reader)
        {
            NameOffset              = reader.ReadUInt32();
            DescriptionNameOffset   = reader.ReadUInt32();
            Flags                   = (MOGPFlags)reader.ReadUInt32();
            BoundingBox1            = reader.ReadC3Vector();
            BoundingBox2            = reader.ReadC3Vector();
            MOPRIndex               = reader.ReadUInt16();
            MOPRCount               = reader.ReadUInt16();
            NumBatchesA             = reader.ReadUInt16();
            NumBatchesB             = reader.ReadUInt16();
            NumBatchesC             = reader.ReadUInt32();

            for (var i = 0; i < 4; ++i)
                FogIndices[i] = reader.ReadByte();

            LiquidType              = reader.ReadUInt32();
            WMOGroupId              = reader.ReadUInt32();
            Unk1                    = reader.ReadUInt32();
            Unk2                    = reader.ReadUInt32();
        }
    }
}
