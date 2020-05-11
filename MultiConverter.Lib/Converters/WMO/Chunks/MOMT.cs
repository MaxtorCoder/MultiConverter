using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using MultiConverter.Lib.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOMT : IChunk
    {
        public string Signature => "MOMT";
        public uint Length => (uint)Write().Length;
        public uint Order => 3;

        /// <summary>
        /// List of all filenames
        /// </summary>
        public Dictionary<string, int> Filenames = new Dictionary<string, int>();

        /// <summary>
        /// List of <see cref="MOMTEntry"/>
        /// </summary>
        public List<MOMTEntry> MOMTs = new List<MOMTEntry>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var textureOffset = 0;
                var momtSize = inData.Length / 64;

                for (var i = 0; i < momtSize; ++i)
                {
                    var momt = new MOMTEntry
                    {
                        Flags1          = reader.ReadUInt32(),
                        ShaderType      = reader.ReadUInt32(),
                        BlendMode       = reader.ReadUInt32(),
                        TextureId1      = reader.ReadUInt32(),
                        SidnColor       = reader.ReadCArgb(),
                        FrameSidnColor  = reader.ReadCArgb(),
                        TextureId2      = reader.ReadUInt32(),
                        DiffColor       = reader.ReadCArgb(),
                        GroundType      = reader.ReadUInt32(),
                        TextureId3      = reader.ReadUInt32(),
                        Color           = reader.ReadCArgb(),
                        Flags2          = reader.ReadUInt32()
                    };

                    for (var j = 0; j < 4; ++j)
                        momt.RunTimeData[j] = reader.ReadUInt32();

                    if (momt.TextureId1 != 0)
                    {
                        var filename = Listfile.LookupFilename(momt.TextureId1, ".wmo").Replace("/", "\\") + "\0";
                        if (!Filenames.ContainsKey(filename))
                        {
                            Filenames.Add(filename, textureOffset);
                            textureOffset += filename.Length;
                        }

                        momt.TextureId1 = (uint)Filenames[filename];
                    }

                    if (momt.TextureId2 != 0)
                    {
                        var filename = Listfile.LookupFilename(momt.TextureId2, ".wmo").Replace("/", "\\") + "\0";
                        if (!Filenames.ContainsKey(filename))
                        {
                            Filenames.Add(filename, textureOffset);
                            textureOffset += filename.Length;
                        }

                        momt.TextureId2 = (uint)Filenames[filename];
                    }

                    if (momt.TextureId3 != 0)
                    {
                        var filename = Listfile.LookupFilename(momt.TextureId3, ".wmo").Replace("/", "\\") + "\0";
                        if (!Filenames.ContainsKey(filename))
                        {
                            Filenames.Add(filename, textureOffset);
                            textureOffset += filename.Length;
                        }

                        momt.TextureId3 = (uint)Filenames[filename];
                    }

                    MOMTs.Add(momt);
                }
            }

            var motx = new MOTX { Filenames = Filenames.Keys.ToList() };
            WMOFile.Chunks.Add(motx);
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var momt in MOMTs)
                {
                    writer.Write(momt.Flags1);
                    writer.Write(momt.ShaderType);
                    writer.Write(momt.BlendMode);
                    writer.Write(momt.TextureId1);
                    writer.WriteCArgb(momt.SidnColor);
                    writer.WriteCArgb(momt.FrameSidnColor);
                    writer.Write(momt.TextureId2);
                    writer.WriteCArgb(momt.DiffColor);
                    writer.Write(momt.GroundType);
                    writer.Write(momt.TextureId3);
                    writer.WriteCArgb(momt.Color);
                    writer.Write(momt.Flags2);

                    for (var i = 0; i < 4; i++)
                        writer.Write(momt.RunTimeData[i]);
                }

                return stream.ToArray();
            }
        }
    }
}
