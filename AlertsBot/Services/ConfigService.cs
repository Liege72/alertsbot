using Newtonsoft.Json;

namespace AlertsBot.Services
{
    public class Config
    {
        [JsonProperty("token")]
        public string? BotToken { get; set; }

        [JsonProperty("mongo_connection_string")]
        public string? MongoConnectionString { get; set; }

        [JsonProperty("database_name")]
        public string? DatabaseName { get; set; }

        [JsonProperty("collection_name")]
        public string? CollectionName { get; set; }

        [JsonProperty("custom_status")]
        public string? CustomStatus { get; set; }

        [JsonProperty("admin_users")]
        public string[]? AdminUsers { get; set; }
    }

    public abstract class ConfigService
    {
        private const string ConfigPath = @"./Config.jsonc";

        public static Config? Config { get; private set; }

        public static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException("No config file found, please make a config file in the current directory!");

            var json = File.ReadAllText(ConfigPath);

            Config = JsonConvert.DeserializeObject<Config>(json);
        }

        public static void SaveConfig(Config? conf)
        {
            var json = JsonConvert.SerializeObject(conf, Formatting.Indented);

            File.WriteAllText(ConfigPath, json);

            Config = conf;
        }
    }
}
