using MultiConverter.Lib.Converters.Base;
using MultiConverter.Lib.Converters.WMO.Entries;
using MultiConverter.Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MODD : IChunk
    {
        public string Signature => "MODD";
        public uint Length => (uint)Write().Length;
        public uint Order => 15;

        /// <summary>
        /// List of <see cref="MODDEntry"/>
        /// </summary>
        public List<MODDEntry> MODDs = new List<MODDEntry>();

        /// <summary>
        /// List of all filenames
        /// </summary>
        public Dictionary<string, uint> Filenames = new Dictionary<string, uint>();

        private uint doodadOffset = 0;

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var moddSize = inData.Length / 40;
                for (var i = 0; i < moddSize; ++i)
                {
                    var finalNameBytes = new byte[4];
                    var nameOffsetBytes = reader.ReadBytes(3);
                    Buffer.BlockCopy(nameOffsetBytes, 0, finalNameBytes, 0, 3);

                    var modd = new MODDEntry
                    {
                        NameIndex   = BitConverter.ToInt32(finalNameBytes, 0),
                        Flags       = reader.ReadByte(),
                        Position    = reader.ReadC3Vector(),
                        Rotation    = reader.ReadC3Vector(),
                        RotationW   = reader.ReadSingle(),
                        Scale       = reader.ReadSingle(),
                        Color       = reader.ReadCArgb()
                    };

                    var filename = AddFilename(MODI.DoodadFileIds[modd.NameIndex]);
                    if (filename != string.Empty)
                    {
                        var idx = Filenames[filename];
                        nameOffsetBytes = BitConverter.GetBytes(idx);

                        Buffer.BlockCopy(nameOffsetBytes, 0, finalNameBytes, 0, 3);
                        modd.NameIndex = BitConverter.ToInt32(finalNameBytes, 0);
                    }
                    else
                        modd.NameIndex = -1;

                    MODDs.Add(modd);
                }
            }
        }

        public byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // Disable doodads.
                if (!WMOFile.DisableDoodads)
                {
                    foreach (var modd in MODDs)
                    {
                        var nameOffsetBytes = BitConverter.GetBytes(modd.NameIndex);
                        var finalNameOffsetBytes = new byte[3];
                        Buffer.BlockCopy(nameOffsetBytes, 0, finalNameOffsetBytes, 0, 3);
                    
                        writer.Write(finalNameOffsetBytes);
                        writer.Write(modd.Flags);
                        writer.WriteC3Vector(modd.Position);
                        writer.WriteC3Vector(modd.Rotation);
                        writer.Write(modd.RotationW);
                        writer.Write(modd.Scale);
                        writer.WriteCArgb(modd.Color);
                    }
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
                    Filenames.Add(textureFilename, doodadOffset);
                    doodadOffset += (uint)textureFilename.Length;
                }

                return textureFilename;
            }

            return string.Empty;
        }
    }
}
