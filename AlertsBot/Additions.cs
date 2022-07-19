using AlertsBot.Services;
using Discord;
using MongoDB.Driver;
using static AlertsBot.Services.DatabaseService;
using static AlertsBot.Handlers.AllCommandHandler;
using System.Globalization;
using Discord.WebSocket;

namespace Additions
{
    public static class Additions
    {
        public static async Task SendAlertAsync(this SocketTextChannel contextChannel, string title, string decription, string imageUrl = null, string footer = null)
        {
            ConfigService.LoadConfig();
            var client = new MongoClient(ConfigService.Config.MongoCS);
            var db = client.GetDatabase(ConfigService.Config.DatabaseName);
            var collection = db.GetCollection<ServerSettings>(ConfigService.Config.CollectionName);

            var enabled = collection.Find(x => x.Status == true).ToList();
            int sentNum = 0;

            foreach (var item in enabled)
            {
                var channel = _client.GetGuild(item.GuildID).GetTextChannel(item.ChannelID);
                var roleId = item.RoleID;
                var color = item.Color;

                int r, g, b = 0;
                r = int.Parse(color.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                g = int.Parse(color.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                b = int.Parse(color.Substring(4, 2), NumberStyles.AllowHexSpecifier);

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
                    await contextChannel.SendMessageAsync($"The provided image URL was invalid so I cancelled the alert!");
                    return;
                }

                if (footer != null)
                    embed.WithFooter(footer);

                try
                {
                    if (roleId == 0)
                    {
                        await channel.SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        await channel.SendMessageAsync($"<@&{roleId}>", embed: embed.Build());
                    }
                    sentNum++;
                }
                catch
                {
                    var user = _client.GetUser(item.UserID);
                    await user.SendMessageAsync($"Hey, I wasn't able to send my latest alert. Can you make sure I have access to send messages in <#{item.ChannelID}>?");
                    continue;
                }

                await contextChannel.SendMessageAsync($"Sent alert to {sentNum} servers!");
            }
        }

        public static async Task SendAlertAsync(this SocketModal modal, string title, string decription, string imageUrl = null, string footer = null)
        {
            await modal.RespondAsync("Sending alert...");

            ConfigService.LoadConfig();
            var client = new MongoClient(ConfigService.Config.MongoCS);
            var db = client.GetDatabase(ConfigService.Config.DatabaseName);
            var collection = db.GetCollection<ServerSettings>(ConfigService.Config.CollectionName);

            var enabled = collection.Find(x => x.Status == true).ToList();
            int sentNum = 0;

            foreach (var item in enabled)
            {
                var channel = _client.GetGuild(item.GuildID).GetTextChannel(item.ChannelID);
                var roleId = item.RoleID;
                var color = item.Color;

                int r, g, b = 0;
                r = int.Parse(color.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                g = int.Parse(color.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                b = int.Parse(color.Substring(4, 2), NumberStyles.AllowHexSpecifier);

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
                    {
                        await channel.SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        await channel.SendMessageAsync($"<@&{roleId}>", embed: embed.Build());
                    }
                    sentNum++;
                }
                catch
                {
                    var user = _client.GetUser(item.UserID);
                    await user.SendMessageAsync($"Hey, I wasn't able to send my latest alert. Can you make sure I have access to send messages in <#{item.ChannelID}>?");
                }

                await modal.ModifyOriginalResponseAsync(x => x.Content = $"Sent alert to {sentNum} servers!");
            }
        }
    }
}
