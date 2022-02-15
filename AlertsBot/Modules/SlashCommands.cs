using AlertsBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Globalization;
using static AlertsBot.Services.DatabaseService;

namespace AlertsBot.Modules
{
    public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("setup", "Use this command to setup alerts in the provided channel.")]
        public async Task SetupAsync([Summary(description: "The channel you want alerts to be sent to")] SocketTextChannel channel)
        {
            var interaction = Context.Interaction as SocketSlashCommand;
            var user = Context.User as SocketGuildUser;
            var guild = Context.Guild.Id;

            if (!user.GuildPermissions.Administrator && user.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            var settings = ServerSettings.GetOrCreateServerSettings(guild);
            settings.ChannelID = channel.Id;
            settings.GuildID = guild;
            settings.UserID = user.Id;
            settings.RoleID = 0;
            settings.Color = "ffff00";
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Saved the GuildID and ChannelID to database! *To set up a role to mention, use the `!mentionrole <role>` command*");
        }

        [SlashCommand("mentionrole", "The provided role will be mentioned for all future alerts.")]
        public async Task MentionRoleAsync([Summary(description: "The role you want to be mentioned for alerts")] SocketRole role)
        {
            var interaction = Context.Interaction as SocketSlashCommand;
            var user = Context.User as SocketGuildUser;
            var guild = Context.Guild.Id;

            if (!user.GuildPermissions.Administrator && user.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            var settings = ServerSettings.GetServerSettings(guild);
            settings.RoleID = role.Id;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Messages will now mention the `{role}` role!");
        }

        [SlashCommand("color", "Sets the provided color for alerts for the provided guild.")]
        public async Task SetColorAsync([Summary(description: "The id of the guild you want to change alert color for")] ulong guildid, [Summary(description: "The color you want alerts to be for the provided guild")] string hex)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            if (Context.User.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            if (guildid.ToString() == null)
                await interaction.RespondAsync($"🔴 **Error:** Please provide a guild id!", ephemeral: true);

            if (hex.Contains("#"))
                hex.Replace("#", "");

            Color color = new Color(255, 255, 0);

            try
            {
                int r, g, b = 0;

                r = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                g = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                b = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);

                color = new Color(r, g, b);
            }
            catch { await interaction.RespondAsync($"🔴 **Error:** Please provide the valid hex value!", ephemeral: true); }

            var settings = ServerSettings.GetServerSettings(guildid);
            settings.Color = color.ToString();
            settings.Status = true;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}` with the color: `{hex}`!");
        }

        [SlashCommand("enable", "Enables alerts on the provided guild.")]
        public async Task EnableAlertsAsync([Summary(description: "The id for the guild you want to enable alerts on")] ulong guildid)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            if (Context.User.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            try { var guild = Context.Client.GetGuild(guildid); }
            catch { await interaction.RespondAsync($"🔴 **Error:** Please provide a valid guild id!", ephemeral: true); }

            var settings = ServerSettings.GetServerSettings(guildid);
            settings.Status = true;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}`!");
        }

        [SlashCommand("disable", "Disable alerts on the provided guild.")]
        public async Task DisableAlertsAsync([Summary(description: "The id for the guild you want to disable alerts on")] ulong guildid)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            if (Context.User.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            try { var guild = Context.Client.GetGuild(guildid); }
            catch { await interaction.RespondAsync($"🔴 **Error:** Please provide a valid guild id!", ephemeral: true); }

            var settings = ServerSettings.GetServerSettings(guildid);
            settings.Status = false;
            settings.SaveThis();
            await interaction.RespondAsync($"🟢 **Success:** Alerts will no longer be sent on the guild with id: `{guildid}`!");
        }

        [SlashCommand("send", "Sends an alert to all enabled guilds.")]
        public async Task SendAlertAsync()
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            if (Context.User.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);
            else
            {
                var modal = new ModalBuilder();
                modal.WithTitle("New Alert");
                modal.AddTextInput("What do you want the alert to say?", "alert_content", TextInputStyle.Paragraph, "Alert Content", 1, 2000, true);
                modal.AddTextInput("What would you like the footer to say?", "footer_content", TextInputStyle.Short, "Footer", null, null, true);
                modal.WithCustomId("alert");
                await interaction.RespondWithModalAsync(modal.Build());
            }
        }

        [SlashCommand("servers", "Lists the servers that the bot is on.")]
        public async Task ListServersAsync()
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            if (Context.User.Id != ConfigService.Config.AdminUser)
                await interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);

            var guilds = Context.Client.Guilds;
            var guildsWithIds = new List<string>();
            foreach (var guild in guilds)
                guildsWithIds.Add($"{guild.Name} ({guild.Id})");

            var count = guilds.Count();
            for (int i = 0; i < guilds.Count; i += 25)
            {
                var embed = new EmbedBuilder();
                if (count >= 25)
                    embed.WithDescription(string.Join("\n", guildsWithIds.Take(25)));
                else
                    embed.WithDescription(string.Join("\n", guildsWithIds.Take(count)));
                embed.WithColor(Color.Blue);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                count -= 25;
            }
        }
    }
}
