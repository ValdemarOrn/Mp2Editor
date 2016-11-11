using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mp2Editor
{
    public class ConfigSettings
    {
        public static string DefaultConfigFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config.json");

        public KeyValuePair<int, string>? MidiInput { get; set; }
        public KeyValuePair<int, string>? MidiOutput { get; set; }
        public int MidiChannel { get; set; }
        public bool AutoUpdate { get; set; }
        public bool LoadOnProgramSelect { get; set; }

        public void SaveToFile(string file = null)
        {
            file = file ?? DefaultConfigFile;
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(file, json);
        }

        public static ConfigSettings LoadFromFile(string file = null)
        {
            file = file ?? DefaultConfigFile;
            if (!File.Exists(file))
                return new ConfigSettings();

            var json = File.ReadAllText(file);
            var obj = JsonConvert.DeserializeObject<ConfigSettings>(json);
            return obj;
        }
    }
}
