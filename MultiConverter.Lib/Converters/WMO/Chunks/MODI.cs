using MultiConverter.Lib.Converters.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MODI : IChunk
    {
        public string Signature => "MODI";
        public uint Length => 0;
        public uint Order => 0;

        public static uint[] DoodadFileIds;
        public static Dictionary<uint, string> DoodadNames = new Dictionary<uint, string>();

        public void Read(byte[] inData)
        {
            using (var stream = new MemoryStream(inData))
            using (var reader = new BinaryReader(stream))
            {
                var modiSize = inData.Length / 4;
                DoodadFileIds = new uint[modiSize];

                for (var i = 0; i < modiSize; ++i)
                {
                    var filedataId = reader.ReadUInt32();
                    var filename = Listfile.LookupFilename(filedataId, ".wmo", "m2").Replace("m2", "mdx").Replace("/", "\\") + "\0";

                    if (!DoodadNames.ContainsKey(filedataId))
                        DoodadNames.Add(filedataId, filename);

                    DoodadFileIds[i] = filedataId;
                }
            }

            if (WMOFile.DisableDoodads)
                WMOFile.Chunks.Add(new MODN());
            else
                WMOFile.Chunks.Add(new MODN { DoodadFilenames = DoodadNames.Values.ToList() });
        }

        public byte[] Write() => new byte[0];
    }
}
