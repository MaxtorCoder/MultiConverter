using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MultiConverterLib
{
    public static class Listfile
    {
        private static WebClient webClient = new WebClient();
        private static string buildConfig = "";
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

            // Check latest buildconfig.
            using (var stream = new MemoryStream(webClient.DownloadData("http://us.patch.battle.net:1119/wow_beta/versions")))
            using (var reader = new StreamReader(stream))
            {
                // Read useless lines.
                reader.ReadLine();
                reader.ReadLine();

                var line = reader.ReadLine();
                var array = line.Split('|');

                // second element of the array
                // us|3f483ee25f283e9072d1a9dceb0160c2|230ddf963c980e2d5ec9882c2a8a00ce||32861|8.3.0.32861|a96756c514489774e38ef1edbc17dcc5
                buildConfig = array[1];
            }
        }

        public static string LookupFilename(uint id, string extension, string modelname, string downloadExtension = "blp")
        {
            // Lookup the Id in the listfile, if it does not exist
            // download and place in blp/<extension>/<modelname>/<blpname>
            if (FiledataPair.TryGetValue(id, out string filename))
            {
                Console.WriteLine($"Filename: {filename} (Length: {filename.Length} ID: {id})");
                return filename;
            }
            else
            {
                if (id != 0)
                {
                    var newFilename = $"{id}.{downloadExtension}";
                    var newExtension = extension.Remove(0, 1);
                    var pathName = $"Unk/{downloadExtension}/{newExtension}/{modelname}/{newFilename}";

                    if (!Directory.Exists($"Unk/{downloadExtension}"))
                        Directory.CreateDirectory($"Unk/{downloadExtension}");
                    if (!Directory.Exists($"Unk/{downloadExtension}/{newExtension}"))
                        Directory.CreateDirectory($"Unk/{downloadExtension}/{newExtension}");
                    if (!Directory.Exists($"Unk/{downloadExtension}/{newExtension}/{modelname}"))
                        Directory.CreateDirectory($"Unk/{downloadExtension}/{newExtension}/{modelname}");

                    if (File.Exists(pathName))
                    {
                        Console.WriteLine($"Filename: {pathName} (Length: {pathName.Length} ID: {id})");
                    }
                    else
                    {
                        Console.WriteLine($"Downloading: {newFilename} (Id: {id})");
                        webClient.DownloadFile($"https://wow.tools/casc/file/fdid?buildconfig={buildConfig}&filename={newFilename}&filedataid={id}", pathName);
                    }

                    return pathName;
                }
            }

            return string.Empty;
        }
    }
}
