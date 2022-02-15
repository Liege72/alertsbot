using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using AlertsBot.Services;

namespace AlertsBot.Services
{
    public class DatabaseService
    {
        static MongoClient Client = new MongoClient(ConfigService.Config.MongoCS);

        static IMongoDatabase Database
            => Client.GetDatabase(ConfigService.Config.DatabaseName);

        public static IMongoCollection<ServerSettings> Settings
                => Database.GetCollection<ServerSettings>(ConfigService.Config.CollectionName);

        [BsonIgnoreExtraElements]
        public class ServerSettings
        {
            public ulong GuildID { get; set; }
            public ulong RoleID { get; set; }
            public ulong UserID { get; set; }
            public ulong ChannelID { get; set; }
            public bool Status { get; set; } = false;
            public string Color { get; set; } = "ffff00";

            public static ServerSettings GetServerSettings(ulong guildid)
            {
                var result = Settings.Find(x => x.GuildID == guildid);

                if (result.Any())
                    return result.First();
                else
                    return null;
            }

            public ServerSettings(ulong guildid)
            {
                this.GuildID = guildid;
                SaveThis();
            }

            public static ServerSettings GetOrCreateServerSettings(ulong guildid)
            {
                var settings = GetServerSettings(guildid);

                if (settings == null)
                    return new ServerSettings(guildid);
                else
                    return settings;
            }
            public void SaveThis()
                => Settings.ReplaceOne(x => x.GuildID == this.GuildID, this, new ReplaceOptions() { IsUpsert = true });
        }
    }
}
