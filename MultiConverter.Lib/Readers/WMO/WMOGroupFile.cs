using CASCLib;
using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Chunks;
using MultiConverter.Lib.Readers.WMO.Chunks.GroupChunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Readers.WMO
{
    public class WMOGroupFile : IConverter
    {
        public static CASCHandler CascHandler;
        public static Dictionary<string, ChunkBase> Chunks = new Dictionary<string, ChunkBase>();

        public void Read(Stream inData)
        {
            // Clear chunks to prevent double data.
            Chunks.Clear();

            using (var reader = new BinaryReader(inData))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (WMOChunkId)reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    var chunkData = new byte[chunkSize];
                    inData.Read(chunkData, 0, (int)chunkSize);

                    ChunkBase chunk = null;
                    switch (chunkId)
                    {
                        case WMOChunkId.MVER: chunk = new MVER(); break;
                        case WMOChunkId.MOGP: chunk = new MOGP(); break;
                        default: 
                            // Console.WriteLine($"Skipping {chunkId} (0x{(uint)chunkId:X})"); 
                            break;
                    }

                    if (chunk != null)
                    {
                        chunk.Read(chunkData);
                        Chunks.Add(chunk.Signature, chunk);
                    }
                }

                // Close the streams so they can be written.
                reader.Close();
            }
        }

        public void Write(string filename)
        {
            var newChunks = Chunks.OrderBy(x => x.Value.Order).ToList();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var chunk in newChunks)
                {
                    var reversedSignature = chunk.Value.Signature.Reverse().ToArray();

                    foreach (var signatureChar in reversedSignature)
                        writer.Write(signatureChar);

                    writer.Write(chunk.Value.Length);
                    writer.Write(chunk.Value.Write());
                }

                File.WriteAllBytes(filename, stream.ToArray());
            }
        }

        public ChunkBase GetChunk(string name)
        {
            Chunks.TryGetValue(name, out var chunk);
            return chunk;
        }
    }
}
