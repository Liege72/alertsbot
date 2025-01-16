using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AlertsBot.Services
{
    public abstract class DatabaseService
    {
        private static readonly MongoClient Client = new MongoClient(ConfigService.Config.MongoConnectionString);

        private static readonly IMongoDatabase Database = Client.GetDatabase(ConfigService.Config.DatabaseName);

        private static readonly IMongoCollection<ServerSettings> Settings = 
            Database.GetCollection<ServerSettings>(ConfigService.Config.CollectionName);

        [BsonIgnoreExtraElements]
        public class ServerSettings
        {
            [BsonElement("guild_id")]
            public ulong GuildId { get; set; }
            
            [BsonElement("role_id")]
            public ulong RoleId { get; set; }
            
            [BsonElement("user_id")]
            public ulong UserId { get; set; }
            
            [BsonElement("channel_id")]
            public ulong ChannelId { get; set; }
            
            [BsonElement("status")]
            public bool Status { get; set; } = false;
            
            [BsonElement("color")]
            public string Color { get; set; } = "ffff00";

            public static ServerSettings? GetServerSettings(ulong guildid)
            {
                var result = Settings.Find(x => x.GuildId == guildid);
                return result.Any() ? result.First() : null;
            }

            private ServerSettings(ulong guildid)
            {
                GuildId = guildid;
                SaveThis();
            }

            public static ServerSettings GetOrCreateServerSettings(ulong guildid)
            {
                var settings = GetServerSettings(guildid);
                return settings ?? new ServerSettings(guildid);
            }
            
            public void SaveThis()
                => Settings.ReplaceOne(x => x.GuildId == this.GuildId, this, new ReplaceOptions() { IsUpsert = true });
        }
    }
}
