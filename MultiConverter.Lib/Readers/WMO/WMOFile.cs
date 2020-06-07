using CASCLib;
using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Chunks;
using MultiConverter.Lib.RenderingObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Readers.WMO
{
    public class WMOFile : IConverter
    {
        public uint WMOFileDataId = 0u;
        public static CASCHandler CascHandler;
        public static Dictionary<string, ChunkBase> Chunks = new Dictionary<string, ChunkBase>();

        public void ReadWMO(CASCHandler handler, string filename)
        {
            CascHandler = handler;
            if (Listfile.TryGetFileDataId(filename, out var fileDataId) && handler.FileExists((int)fileDataId))
            {
                using (var stream = handler.OpenFile((int)fileDataId))
                {
                    WMOFileDataId = fileDataId;
                    Read(stream);
                }
            }
        }

        public void Read(Stream data)
        {
            // Clear chunks to prevent double data.
            Chunks.Clear();

            using (var reader = new BinaryReader(data))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunkId = (WMOChunkId)reader.ReadUInt32();
                    var chunkSize = reader.ReadUInt32();

                    var chunkData = new byte[chunkSize];
                    data.Read(chunkData, 0, (int)chunkSize);

                    ChunkBase chunk = null;
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
                        case WMOChunkId.GFID: chunk = new GFID(); break;
                        default: Console.WriteLine($"Skipping {chunkId} (0x{(uint)chunkId:X})"); break;
                    }

                    if (chunk != null)
                    {
                        chunk.Read(chunkData);
                        Chunks.Add(chunk.Signature, chunk);
                    }
                }

                // Add mandatory chunks.
                Chunks.Add("MOSB", new MOSB());
                Chunks.Add("MOVV", new MOVV());
                Chunks.Add("MOVB", new MOVB());

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
                    // Ignore this chunk because it does not have to be written
                    if (chunk.Value.Signature == "MODI" || chunk.Value.Signature == "GFID")
                        continue;

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
