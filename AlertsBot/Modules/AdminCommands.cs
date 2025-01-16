using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using static AlertsBot.Services.DatabaseService;

namespace AlertsBot.Modules
{
    [RequireConfigUser]
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("enable", "Enables alerts on the provided guild.")]
        public async Task EnableAlertsAsync([Summary(description: "The id for the guild you want to enable alerts on")] string guildid)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            ulong id = 99;
            try { id = Convert.ToUInt64(guildid); Context.Client.GetGuild(id); }
            catch { await interaction.RespondAsync($"🔴 **Error:** Please provide a valid guild id!", ephemeral: true); return; }

            var settings = ServerSettings.GetServerSettings(id);
            settings.Status = true;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}`!");
        }

        [SlashCommand("disable", "Disable alerts on the provided guild.")]
        public async Task DisableAlertsAsync([Summary(description: "The id for the guild you want to disable alerts on")] string guildid)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            ulong id = 99;
            try { id = Convert.ToUInt64(guildid); Context.Client.GetGuild(id); }
            catch { await interaction.RespondAsync($"🔴 **Error:** Please provide a valid guild id!", ephemeral: true); return; }

            var settings = ServerSettings.GetServerSettings(id);
            settings.Status = false;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Alerts will no longer be sent on the guild with id: `{guildid}`!");
        }

        [SlashCommand("send", "Sends an alert to all enabled guilds.")]
        public async Task SendAlertAsync()
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            var modal = new ModalBuilder();
            modal.WithTitle("New Alert");
            modal.WithCustomId("alert");
            modal.AddTextInput("What should the alert title be?", "alert_title", TextInputStyle.Short, "Alert Title", 1, 120, true);
            modal.AddTextInput("What do you want the alert to say?", "alert_content", TextInputStyle.Paragraph, "Alert Content", 1, 2000, true);
            modal.AddTextInput("What would you like the image to be?", "alert_image", TextInputStyle.Short, "Image URL", required: false);
            modal.AddTextInput("What would you like the footer to say?", "alert_footer", TextInputStyle.Short, "Footer", 1, 80, false);
            await interaction.RespondWithModalAsync(modal.Build());
        }

        [SlashCommand("servers", "Lists the servers that the bot is on.")]
        public async Task ListServersAsync()
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            var guilds = Context.Client.Guilds;
            var guildsWithIds = guilds.Select(guild => $"{guild.Name} ({guild.Id})").ToList();

            var count = guilds.Count();
            for (var i = 0; i < guilds.Count; i += 25)
            {
                var embed = new EmbedBuilder();
                embed.WithDescription(count >= 25
                    ? string.Join("\n", guildsWithIds.Take(25))
                    : string.Join("\n", guildsWithIds.Take(count)));
                embed.WithColor(Color.Blue);
                await interaction.RespondAsync("List is below");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                count -= 25;
            }
        }
    }
}
