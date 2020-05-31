using MultiConverter.Lib.Util;
using MultiConverter.Lib.Constants;
using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries.GroupEntries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Readers.WMO.Chunks.GroupChunks
{
    public class MOGP : ChunkBase
    {
        public override string Signature => "MOGP";
        public override uint Length => (uint)Write().Length;

        public MOGPEntry MOGPEntry = new MOGPEntry();

        public override void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                MOGPEntry.Read(reader);

                if (MOGPEntry.Flags.HasFlag(MOGPFlags.Flag_0x40000000))
                    MOGPEntry.TextureCoords = new MOTVEntry[3][];
                else
                    MOGPEntry.TextureCoords = new MOTVEntry[2][];

                var motvCount = 0;

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (WMOChunkId)reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    switch (chunkId)
                    {
                        case WMOChunkId.MONR:
                            MOGPEntry.Normals = ReadNormals(reader, chunkSize);
                            break;
                        case WMOChunkId.MOPY:
                            MOGPEntry.MaterialInfo = ReadMaterialInfo(reader, chunkSize);
                            break;
                        case WMOChunkId.MOTV:
                            MOGPEntry.TextureCoords[motvCount++] = ReadTextureCoords(reader, chunkSize);
                            break;
                        case WMOChunkId.MOVI:
                            MOGPEntry.Indices = ReadIndices(reader, chunkSize);
                            break;
                        case WMOChunkId.MOVT:
                            MOGPEntry.Vertices = ReadVertices(reader, chunkSize);
                            break;
                        case WMOChunkId.MOBA:
                            MOGPEntry.RenderBatches = ReadRenderBatches(reader, chunkSize);
                            break;
                        default:
                            // Console.WriteLine($"Skipping {chunkId} (0x{(uint)chunkId})");
                            reader.BaseStream.Position += chunkSize;
                            break;
                    }
                }
            }
        }

        public MOVIEntry[] ReadIndices(BinaryReader reader, uint size)
        {
            var numMaterials = size / 2;
            var indices = new MOVIEntry[numMaterials];

            for (var i = 0; i < numMaterials; i++)
                indices[i] = reader.Read<MOVIEntry>();

            return indices;
        }

        public MOPYEntry[] ReadMaterialInfo(BinaryReader reader, uint size)
        {
            var numMaterials = size / 2;
            var materials = new MOPYEntry[numMaterials];

            for (var i = 0; i < numMaterials; i++)
                materials[i] = reader.Read<MOPYEntry>();

            return materials;
        }

        public MOVTEntry[] ReadVertices(BinaryReader reader, uint size)
        {
            var numMaterials = size / 12;
            var vertices = new MOVTEntry[numMaterials];

            for (var i = 0; i < numMaterials; i++)
                vertices[i] = reader.Read<MOVTEntry>();

            return vertices;
        }

        public MOTVEntry[] ReadTextureCoords(BinaryReader reader, uint size)
        {
            var numMaterials = size / 8;
            var textureCoords = new MOTVEntry[numMaterials];

            for (var i = 0; i < numMaterials; i++)
                textureCoords[i] = reader.Read<MOTVEntry>();

            return textureCoords;
        }

        public MONREntry[] ReadNormals(BinaryReader reader, uint size)
        {
            var numMaterials = size / 12;
            var textureCoords = new MONREntry[numMaterials];

            for (var i = 0; i < numMaterials; i++)
                textureCoords[i] = reader.Read<MONREntry>();

            return textureCoords;
        }

        public MOBAEntry[] ReadRenderBatches(BinaryReader reader, uint size)
        {
            var numBatches = size / 24;
            var batches = new MOBAEntry[numBatches];

            for (var i = 0; i < numBatches; i++)
                batches[i] = reader.Read<MOBAEntry>();

            return batches;
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                return stream.ToArray();
            }
        }
    }
}
