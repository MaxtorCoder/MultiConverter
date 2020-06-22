using MultiConverter.Lib.Converters.Base;
// using MultiConverter.Lib.Converters.ADT.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MultiConverter.Lib.Converters.ADT
{
    public class ADTFile : IConverter
    {
        public static List<IChunk> Chunks = new List<IChunk>();
        public static Dictionary<string, byte[]> ChunkData = new Dictionary<string, byte[]>();
        public static Dictionary<string, (uint order, bool disabled)> ChunkExtraData = new Dictionary<string, (uint order, bool disabled)>();

        public static bool DisableDoodads = false;
        public static bool DisableWMOs = false;

        public ADTFile(string objFilename, string texFilename)
        {

        }

        public void Read(byte[] inData)
        {
            // Clear chunks to prevent double data.
            Chunks.Clear();

            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var chunkOrder = 0u;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = new string(reader.ReadChars(4).Reverse().ToArray());
                    var chunkSize = reader.ReadUInt32();

                    var chunkData = new byte[chunkSize];
                    Buffer.BlockCopy(stream.ToArray(), (int)reader.BaseStream.Position, chunkData, 0, (int)chunkSize);

                    IChunk chunk = null;
                    switch (chunkId)
                    {
                        default:
                            Console.WriteLine($"Skipping {chunkId}"); 
                            break;
                    }

                    if (chunk != null)
                        chunk.Read(chunkData);

                    ChunkExtraData.Add(chunkId, (chunkOrder, false));
                    ChunkData.Add(chunkId, chunk != null ? chunk.Write() : chunkData);

                    ++chunkOrder;
                    reader.BaseStream.Position += chunkSize;
                }

                // Close the streams so they can be written.
                reader.Close();
                stream.Close();
            }
        }

        public void Write(string filename)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var chunk in ChunkData)
                {
                    var reversedSignature = chunk.Key.Reverse().ToArray();

                    foreach (var signatureChar in reversedSignature)
                        writer.Write(signatureChar);

                    writer.Write(chunk.Value.Length);
                    writer.Write(chunk.Value);
                }

                File.WriteAllBytes(filename, stream.ToArray());
            }
        }
    }
}
