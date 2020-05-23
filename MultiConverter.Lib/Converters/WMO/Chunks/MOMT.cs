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
        public Dictionary<string, uint> Filenames = new Dictionary<string, uint>();

        /// <summary>
        /// List of <see cref="MOMTEntry"/>
        /// </summary>
        public List<MOMTEntry> MOMTs = new List<MOMTEntry>();

        private uint textureOffset = 0;

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
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

                    momt.Flags1 &= 0xFF;

                    for (var j = 0; j < 4; ++j)
                        momt.RunTimeData[j] = reader.ReadUInt32();

                    var filename = AddFilename(momt.TextureId1);
                    if (filename != string.Empty)
                    {
                        var idx = Filenames[filename];
                        momt.TextureId1 = idx;
                    }
                    else
                        momt.TextureId1 = 0;

                    filename = AddFilename(momt.TextureId2);
                    if (filename != string.Empty)
                    {
                        var idx = Filenames[filename];
                        momt.TextureId2 = idx;
                    }
                    else
                        momt.TextureId2 = 0;

                    filename = AddFilename(momt.TextureId3);
                    if (filename != string.Empty)
                    {
                        var idx = Filenames[filename];
                        momt.TextureId3 = idx;
                    }
                    else
                        momt.TextureId3 = 0;

                    switch (momt.ShaderType)
                    {
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                        case 7:  momt.ShaderType = 6; break;
                        case 9:  momt.ShaderType = 0; break;
                        case 12: momt.ShaderType = 5; break;
                        default:
                            if (momt.ShaderType >= 13)
                                momt.ShaderType = 4;
                            break;
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

        private string AddFilename(uint fdid)
        {
            if (fdid != 0)
            {
                var textureFilename = Listfile.LookupFilename(fdid, ".wmo").Replace('/', '\\') + "\0";

                if (!Filenames.ContainsKey(textureFilename))
                {
                    Filenames.Add(textureFilename, textureOffset);
                    textureOffset += (uint)textureFilename.Length;
                }

                return textureFilename;
            }

            return string.Empty;
        }
    }
}
