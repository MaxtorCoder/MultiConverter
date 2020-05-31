using MultiConverter.Lib.Readers.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiConverter.Lib.Readers.WMO.Chunks
{
    public class MODI : ChunkBase
    {
        public override string Signature => "MODI";
        public override uint Length => 0;
        public override uint Order => 0;

        public static uint[] DoodadFileIds;
        public static Dictionary<uint, string> DoodadNames = new Dictionary<uint, string>();

        public override void Read(byte[] inData)
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

            WMOFile.Chunks.Add("MODN", new MODN { DoodadFilenames = DoodadNames.Values.ToList() });
        }

        public override byte[] Write() => new byte[0];
    }
}
