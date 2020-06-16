using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Converters.WMO
{
    public class WMOFile : IConverter
    {
        public static List<IChunk> Chunks = new List<IChunk>();
        public static bool DisableDoodads = true;

        public void Read(byte[] data)
        {
            // Clear chunks to prevent double data.
            Chunks.Clear();

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (WMOChunkId)reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    var chunkData = new byte[chunkSize];
                    Buffer.BlockCopy(stream.ToArray(), (int)reader.BaseStream.Position, chunkData, 0, (int)chunkSize);

                    IChunk chunk = null;
                    switch (chunkId)
                    {
                        case WMOChunkId.MVER: chunk = new MVER(); break;
                        case WMOChunkId.MOHD: chunk = new MOHD(); break;
                        case WMOChunkId.MOTX: chunk = new MOTX(); break;
                        case WMOChunkId.MOMT: chunk = new MOMT(); break;
                        case WMOChunkId.MOGN: chunk = new MOGN(); break;
                        case WMOChunkId.MOGI: chunk = new MOGI(); break;
                        case WMOChunkId.MOPV: chunk = new MOPV(); break;
                        case WMOChunkId.MOPT: chunk = new MOPT(); break;
                        case WMOChunkId.MOPR: chunk = new MOPR(); break;
                        case WMOChunkId.MOLT: chunk = new MOLT(); break;
                        case WMOChunkId.MODS: chunk = new MODS(); break;
                        case WMOChunkId.MODI: chunk = new MODI(); break;
                        case WMOChunkId.MODD: chunk = new MODD(); break;
                        case WMOChunkId.MFOG: chunk = new MFOG(); break;
                        default: Console.WriteLine($"Skipping {chunkId} (0x{(uint)chunkId:X})"); break;
                    }

                    if (chunk != null)
                    {
                        chunk.Read(chunkData);
                        Chunks.Add(chunk);
                    }

                    reader.BaseStream.Position += chunkSize;
                }

                // Add mandatory chunks.
                Chunks.Add(new MOSB());
                Chunks.Add(new MOVV());
                Chunks.Add(new MOVB());

                // Close the streams so they can be written.
                reader.Close();
                stream.Close();
            }
        }

        public void Write(string filename)
        {
            Chunks = Chunks.OrderBy(x => x.Order).ToList();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var chunk in Chunks)
                {
                    // Ignore this chunk because it does not have to be written
                    if (chunk.Signature == "MODI")
                        continue;

                    var reversedSignature = chunk.Signature.Reverse().ToArray();

                    foreach (var signatureChar in reversedSignature)
                        writer.Write(signatureChar);

                    writer.Write(chunk.Length);
                    writer.Write(chunk.Write());
                }

                File.WriteAllBytes(filename, stream.ToArray());
            }
        }
    }
}
