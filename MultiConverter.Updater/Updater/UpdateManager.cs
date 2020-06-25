using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace MultiConverter.Updater.Updater
{
    public static class UpdateManager
    {
        public const string UpdateUrl = "http://localhost/update.json";
        public const string BetaUpdateUrl = "http://localhost/update_beta.json";

        public static (bool, string) HasUpdates(string versionString, bool hasBeta = false)
        {
            using (var webclient = new WebClient())
            {
                var reponseString = webclient.DownloadString(hasBeta ? BetaUpdateUrl : UpdateUrl);

                var update = JsonConvert.DeserializeObject<Update>(reponseString);
                if (update.VersionString == versionString)
                    return (false, string.Empty);

                return (true, update.VersionString);
            }
        }

        public static string GetChangelog(bool hasBeta = false)
        {
            using (var webclient = new WebClient())
            {
                var reponseString = webclient.DownloadString(hasBeta ? BetaUpdateUrl : UpdateUrl);

                var update = JsonConvert.DeserializeObject<Update>(reponseString);
                return update.Changelog;
            }
        }

        public static void StartUpdater(bool hasBeta = false)
        {
            Process.Start("MultiConverter.Updater.exe", $"-b \"{(hasBeta ? true : false)}\"");
        }
    }
}
