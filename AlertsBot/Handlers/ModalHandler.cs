using AlertsBot.Services;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using System.Globalization;
using static AlertsBot.Services.DatabaseService;

namespace AlertsBot.Handlers
{
    public class ModalHandler
    {
        private readonly DiscordSocketClient _client;

        public ModalHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.ModalSubmitted += ModalSubmitted;
        }

        public async Task ModalSubmitted(SocketModal arg)
        {
            ConfigService.LoadConfig();
            MongoClient Client = new MongoClient(ConfigService.Config.MongoCS);

            var id = arg.Data.CustomId;
            var comps = arg.Data.Components;

            if (id == "modal")
            {
                var alertText = comps.First(x => x.CustomId == "alert_content").Value;
                var footerText = comps.First(x => x.CustomId == "footer_content").Value;

                var database = Client.GetDatabase("Steve-Bots");
                var docs = Settings.Find(x => x.Status == true).ToList();

                int sentTo = 0;

                try
                {
                    foreach (var doc in docs)
                    {
                        var guild = _client.GetGuild(doc.GuildID);
                        var channel = guild.GetTextChannel(doc.ChannelID);
                        var user = await _client.GetUserAsync(doc.UserID);
                        var role = guild.GetRole(doc.RoleID);
                        var color = doc.Color;
                        var botUser = guild.GetUser(_client.CurrentUser.Id);

                        if (!botUser.GetPermissions(channel).ViewChannel)
                        {
                            await user.SendMessageAsync($"I am missing access to the {channel.Name} channel, please give me access to view this channel so I can send alerts!");
                            continue;
                        }

                        if (!botUser.GetPermissions(channel).SendMessages)
                        {
                            await user.SendMessageAsync($"I am missing access to the {channel.Name} channel, please give me access to send messages to this channel so I can send alerts!");
                            continue;
                        }

                        int r, g, b = 0;

                        r = int.Parse(color.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                        g = int.Parse(color.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                        b = int.Parse(color.Substring(4, 2), NumberStyles.AllowHexSpecifier);

                        var embed = new EmbedBuilder();
                        embed.WithColor(new Color(r, g, b));

                        if (role.Id == 0)
                        {
                            await channel.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await channel.SendMessageAsync($"{role.Mention}", false, embed.Build());
                        }
                        sentTo++;
                    }
                }
                catch
                {
                    // could not send
                }

                await arg.RespondAsync($"Sent alert to {sentTo} servers!");
            }
        }
    }
}
