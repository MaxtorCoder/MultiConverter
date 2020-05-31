using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Net.Http.Headers;

namespace CASCLib
{
    public class IndexEntry
    {
        public int Index;
        public int Offset;
        public int Size;
    }

    public class CDNIndexHandler
    {
        private static readonly MD5HashComparer comparer = new MD5HashComparer();
        private Dictionary<MD5Hash, IndexEntry> CDNIndexData = new Dictionary<MD5Hash, IndexEntry>(comparer);

        private CASCConfig config;

        public IReadOnlyDictionary<MD5Hash, IndexEntry> Data => CDNIndexData;
        public int Count => CDNIndexData.Count;

        private CDNIndexHandler(CASCConfig cascConfig)
        {
            config = cascConfig;
        }

        public static CDNIndexHandler Initialize(CASCConfig config)
        {
            var handler = new CDNIndexHandler(config);

            for (int i = 0; i < config.Archives.Count; i++)
            {
                string archive = config.Archives[i];

                if (config.OnlineMode)
                    handler.DownloadIndexFile(archive, i);
                else
                    handler.OpenIndexFile(archive, i);
            }

            return handler;
        }

        private void ParseIndex(Stream stream, int i)
        {
            using (var br = new BinaryReader(stream))
            {
                stream.Seek(-12, SeekOrigin.End);
                int count = br.ReadInt32();
                stream.Seek(0, SeekOrigin.Begin);

                if (count * (16 + 4 + 4) > stream.Length)
                    throw new Exception("ParseIndex failed");

                for (int j = 0; j < count; ++j)
                {
                    MD5Hash key = br.Read<MD5Hash>();

                    if (key.IsZeroed()) // wtf?
                        key = br.Read<MD5Hash>();

                    if (key.IsZeroed()) // wtf?
                        throw new Exception("key.IsZeroed()");

                    IndexEntry entry = new IndexEntry()
                    {
                        Index = i,
                        Size = br.ReadInt32BE(),
                        Offset = br.ReadInt32BE()
                    };
                    CDNIndexData.Add(key, entry);
                }
            }
        }

        private void DownloadIndexFile(string archive, int i)
        {
            try
            {
                var file = config.CDNPath + "/data/" + archive.Substring(0, 2) + "/" + archive.Substring(2, 2) + "/" + archive + ".index";
                var url = "http://" + config.CDNHost + "/" + file;

                using (var fs = OpenFile(url))
                    ParseIndex(fs, i);
            }
            catch (Exception exc)
            {
                throw new Exception($"DownloadFile failed: {archive} - {exc}");
            }
        }

        private void OpenIndexFile(string archive, int i)
        {
            try
            {
                string dataFolder = CASCGame.GetDataFolder(config.GameType);

                string path = Path.Combine(config.BasePath, dataFolder, "indices", archive + ".index");

                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        ParseIndex(fs, i);
                }
                else
                {
                    DownloadIndexFile(archive, i);
                }
            }
            catch (Exception exc)
            {
                throw new Exception($"OpenFile failed: {archive} - {exc}");
            }
        }

        public Stream OpenDataFile(IndexEntry entry)
        {
            var archive = config.Archives[entry.Index];

            var file = config.CDNPath + "/data/" + archive.Substring(0, 2) + "/" + archive.Substring(2, 2) + "/" + archive;
            var url = "http://" + config.CDNHost + "/" + file;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(entry.Offset, entry.Offset + entry.Size - 1);
                var response = httpClient.GetAsync(url).Result;

                return new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
            }
        }

        public Stream OpenDataFileDirect(MD5Hash key)
        {
            var keyStr = key.ToHexString().ToLower();
            var file = config.CDNPath + "/data/" + keyStr.Substring(0, 2) + "/" + keyStr.Substring(2, 2) + "/" + keyStr;
            var url = "http://" + config.CDNHost + "/" + file;

            return OpenFile(url);
        }

        public static Stream OpenConfigFileDirect(CASCConfig cfg, string key)
        {
            var file = cfg.CDNPath + "/config/" + key.Substring(0, 2) + "/" + key.Substring(2, 2) + "/" + key;
            var url = "http://" + cfg.CDNHost + "/" + file;

            return OpenFileDirect(url);
        }

        public static Stream OpenFileDirect(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(url).Result;

                return new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
            }
        }

        private Stream OpenFile(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(url).Result;

                return new MemoryStream(response.Content.ReadAsByteArrayAsync().Result);
            }
        }

        private static long GetFileSize(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetAsync(url).Result;

                return response.Content.ReadAsByteArrayAsync().Result.Length;
            }
        }

        public IndexEntry GetIndexInfo(MD5Hash key)
        {
            if (!CDNIndexData.TryGetValue(key, out IndexEntry result))
                Console.WriteLine("CDNIndexHandler: missing index: {0}", key.ToHexString());

            return result;
        }

        public void Clear()
        {
            CDNIndexData.Clear();
            CDNIndexData = null;

            config = null;
        }
    }
}
