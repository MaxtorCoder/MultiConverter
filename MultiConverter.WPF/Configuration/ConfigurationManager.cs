using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConverter.WPF.Configuration
{
    public static class ConfigurationManager<T>
    {
        public static T Config { get; private set; }

        public static void Initialize(string filename)
        {
            var fileContents = File.ReadAllText(filename);
            Config = JsonConvert.DeserializeObject<T>(fileContents);
        }
    }
}
