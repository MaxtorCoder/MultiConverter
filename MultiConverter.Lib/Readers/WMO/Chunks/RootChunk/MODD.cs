using MultiConverter.Lib.Readers.Base;
using MultiConverter.Lib.Readers.WMO.Entries;
using MultiConverter.Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MODD : ChunkBase
    {
        public override string Signature => "MODD";
        public override uint Length => (uint)Write().Length;
        public override uint Order => 15;

        /// <summary>
        /// List of <see cref="MODDEntry"/>
        /// </summary>
        public List<MODDEntry> MODDs = new List<MODDEntry>();

        public override void Read(byte[] inData)
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
                        NameIndex   = BitConverter.ToUInt32(finalNameBytes, 0),
                        Flags       = reader.ReadByte(),
                        Position    = reader.ReadC3Vector(),
                        Rotation    = reader.ReadC3Vector(),
                        RotationW   = reader.ReadSingle(),
                        Scale       = reader.ReadSingle(),
                        Color       = reader.ReadCArgb()
                    };

                    MODDs.Add(modd);
                }
            }
        }

        public override byte[] Write()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var modd in MODDs)
                {
                    var fileid = MODI.DoodadFileIds[modd.NameIndex];
                    modd.NameIndex = (uint)MODI.DoodadNames.Keys.ToList().IndexOf(fileid);

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

                return stream.ToArray();
            }
        }
    }
}
