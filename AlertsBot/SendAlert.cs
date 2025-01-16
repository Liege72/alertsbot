using System.Globalization;
using AlertsBot.Services;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using static AlertsBot.Services.DatabaseService;
using static AlertsBot.Handlers.AllCommandHandler;

namespace AlertsBot
{
    public static class SendAlert
    {
        public static async Task SendAlertAsync(this SocketModal modal, string title, string decription, string imageUrl = null, string footer = null)
        {
            await modal.RespondAsync("Sending alert...");

            ConfigService.LoadConfig();
            var client = new MongoClient(ConfigService.Config.MongoConnectionString);
            var db = client.GetDatabase(ConfigService.Config.DatabaseName);
            var collection = db.GetCollection<ServerSettings>(ConfigService.Config.CollectionName);

            var enabled = collection.Find(x => x.Status == true).ToList();
            int sentNum = 0;

            foreach (var item in enabled)
            {
                var channel = Client.GetGuild(item.GuildId).GetTextChannel(item.ChannelId);
                var roleId = item.RoleId;
                var color = item.Color;
                
                var r = int.Parse(color.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                var g = int.Parse(color.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                var b = int.Parse(color.Substring(4, 2), NumberStyles.AllowHexSpecifier);

                var embed = new EmbedBuilder();
                embed.WithTitle(title);
                embed.WithDescription(decription);
                embed.WithColor(new Color(r, g, b));

                try
                {
                    if (imageUrl != null)
                        embed.WithImageUrl(imageUrl);
                }
                catch
                {
                    await modal.RespondAsync($"The provided image URL was invalid so I cancelled the alert!");
                    return;
                }

                if (footer != null)
                    embed.WithFooter(footer);

                try
                {
                    if (roleId == 0)
                        await channel.SendMessageAsync(embed: embed.Build());
                    else
                        await channel.SendMessageAsync($"<@&{roleId}>", embed: embed.Build());
                    sentNum++;
                }
                catch
                {
                    var user = Client.GetUser(item.UserId);
                    await user.SendMessageAsync($"Hey, I wasn't able to send my latest alert. Can you make sure I have access to send messages in <#{item.ChannelId}>?");
                }

                await modal.ModifyOriginalResponseAsync(x => x.Content = $"Sent alert to {sentNum} servers!");
            }
        }
    }
}
