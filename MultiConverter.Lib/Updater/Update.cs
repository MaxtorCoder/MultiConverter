using System;
using System.Net;

namespace MultiConverter.Lib.Updater
{
    public class Update
    {
        public string VersionString { get; set; }
        public string DownloadUrl { get; set; }
        public string Changelog { get; set; }

        [NonSerialized]
        public WebClient WebClient;
    }
}
