using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MultiConverter.Lib
{
    public static class Listfile
    {
        private static WebClient webClient = new WebClient();
        private static string listfile = "listfile.csv";
        private static string listUrl = "https://wow.tools/casc/listfile/download/csv/unverified";
        private static Dictionary<uint, string> FiledataPair = new Dictionary<uint, string>();

        public static bool IsInitialized = false;

        public static void Initialize()
        {
            // Download listfile if file does not exist.
            if (!File.Exists(listfile))
                webClient.DownloadFile(listUrl, listfile);
            else
            {
                using (var reader = new StreamReader(listfile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var array = line.Split(';');

                        FiledataPair.Add(uint.Parse(array[0]), array[1]);
                    }

                    IsInitialized = true;
                }
            }
        }

        public static string LookupFilename(uint id, string extension, string downloadExtension = "blp")
        {
            // Lookup the Id in the listfile, if it does not exist
            // place in blp/<extension>/<modelname>/<blpname>
            if (FiledataPair.TryGetValue(id, out string filename))
            {
                //Console.WriteLine($"Filename: {filename} (Length: {filename.Length} ID: {id})");
                return filename;
            }
            else
            {
                if (id != 0)
                {
                    var newFilename = $"{id}.{downloadExtension}";
                    var newExtension = extension.Remove(0, 1);
                    var pathName = $"Unk/{downloadExtension}/{newExtension}/{newFilename}";

                    //Console.WriteLine($"Filename: {pathName} (Length: {pathName.Length} ID: {id})");

                    return pathName;
                }
            }

            return string.Empty;
        }

        public static bool TryGetFileDataId(string filename, out uint fileDataId)
        {
            var cleaned = filename.ToLower().Replace('\\', '/');

            var fdid = 0u;
            Parallel.ForEach(FiledataPair, fileData =>
            {
                if (fileData.Value == filename)
                    fdid = fileData.Key;
            });

            fileDataId = fdid;

            return true;
        }
    }
}
