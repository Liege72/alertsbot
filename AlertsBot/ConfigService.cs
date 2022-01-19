using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertsBot.Services
{
    public class Config
    {
        public string BotToken { get; set; }

        public string MongoCS { get; set; }

        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string Prefix { get; set; }
        public string CustomStatus { get; set; }
        public ulong AdminUser { get; set; }

    }

    public class ConfigService
    {
        public const string ConfigPath = @"./Config.jsonc";

        public static Config Config { get; private set; }

        public static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException("No config file found, please make a config file in the current directory!");

            var json = File.ReadAllText(ConfigPath);

            Config = JsonConvert.DeserializeObject<Config>(json);
        }

        public static void SaveConfig(Config conf)
        {
            var json = JsonConvert.SerializeObject(conf, Formatting.Indented);

            File.WriteAllText(ConfigPath, json);

            Config = conf;
        }
    }
}