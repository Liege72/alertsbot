using System.Globalization;
using AlertsBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace AlertsBot.Modules
{
    [RequirePermissions]
    public class ServerCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("setup", "Use this command to setup alerts in the provided channel.")]
        public async Task SetupAsync(
            [Summary(description: "The channel you want alerts to be sent to")]
            SocketTextChannel channel)
        {
            var interaction = Context.Interaction as SocketSlashCommand;
            var user = Context.User as SocketGuildUser;
            var guild = Context.Guild.Id;

            var settings = DatabaseService.ServerSettings.GetOrCreateServerSettings(guild);
            settings.ChannelId = channel.Id;
            settings.GuildId = guild;
            settings.UserId = user.Id;
            settings.RoleId = 0;
            settings.Color = "ffff00";
            settings.SaveThis();

            await interaction.RespondAsync(
                "🟢 **Success:** Saved the GuildID and ChannelID to database! *To set up a role to mention, use the `!mentionrole <role>` command*");
        }

        [SlashCommand("mentionrole", "The provided role will be mentioned for all future alerts.")]
        public async Task MentionRoleAsync(
            [Summary(description: "The role you want to be mentioned for alerts")]
            SocketRole role)
        {
            var interaction = Context.Interaction as SocketSlashCommand;
            var guild = Context.Guild.Id;

            var settings = DatabaseService.ServerSettings.GetServerSettings(guild);
            settings.RoleId = role.Id;
            settings.SaveThis();

            await interaction.RespondAsync($"🟢 **Success:** Messages will now mention the `{role}` role!");
        }

        [SlashCommand("color", "Sets the provided color for alerts for the provided guild.")]
        public async Task SetColorAsync(
            [Summary(description: "The id of the guild you want to change alert color for")]
            string guildid,
            [Summary(description: "The color you want alerts to be for the provided guild")]
            string hex)
        {
            var interaction = Context.Interaction as SocketSlashCommand;

            ulong id = 99;
            try
            {
                id = Convert.ToUInt64(guildid);
                Context.Client.GetGuild(id);
            }
            catch
            {
                await interaction.RespondAsync($"🔴 **Error:** Please provide a valid guild id!", ephemeral: true);
                return;
            }

            if (hex.Contains("#"))
                hex = hex.Replace("#", "");

            var color = new Color(255, 255, 0);

            try
            {
                int r, g, b = 0;

                r = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                g = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                b = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);

                color = new Color(r, g, b);
            }
            catch
            {
                await interaction.RespondAsync($"🔴 **Error:** Please provide the valid hex value!", ephemeral: true);
            }

            var settings = DatabaseService.ServerSettings.GetServerSettings(id);
            settings.Color = color.ToString().Substring(1);
            settings.Status = true;
            settings.SaveThis();

            await interaction.RespondAsync(
                $"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}` with the color: `{hex}`!");
        }
    }
}