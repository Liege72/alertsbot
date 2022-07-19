using AlertsBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using NextMessageAsync;
using System.Threading.Tasks;
using static AlertsBot.Services.DatabaseService;
using static Additions.Additions;

namespace AlertsBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("setup")]
        public async Task Setup(SocketTextChannel channel = null)
        {
            var user = Context.User as SocketGuildUser;
            var guild = Context.Guild.Id;

            if (!user.GuildPermissions.Administrator)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            if (channel == null)
            {
                await ReplyAsync($"🔴 **Error:** Please mention a channel!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            var settings = ServerSettings.GetOrCreateServerSettings(guild);
            settings.ChannelID = channel.Id;
            settings.GuildID = guild;
            settings.RoleID = 0;
            settings.UserID = user.Id;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Saved the GuildID and ChannelID to database! *To set up a role to mention, use the `!mentionrole <role>` command*");
            return;

        }

        [Command("mentionrole")]
        public async Task MentionRole(SocketRole role = null)
        {
            var user = Context.User as SocketGuildUser;
            var guild = Context.Guild.Id;

            if (!user.GuildPermissions.Administrator)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            if (role == null)
            {
                await ReplyAsync($"🔴 **Error:** Please mention a role!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            var settings = ServerSettings.GetServerSettings(guild);
            settings.RoleID = role.Id;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Messages will now mention the `{role}` role!");
        }

        [Command("enable")]
        public async Task EnableAlerts(string guildid)
        {
            var user = Context.User as SocketGuildUser;

            if (Context.User.Id != ConfigService.Config.AdminUser)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            ulong id = 99;
            try { id = Convert.ToUInt64(guildid); Context.Client.GetGuild(id); }
            catch { await ReplyAsync($"🔴 **Error:** Please provide a guild id!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id)); return; }

            var settings = ServerSettings.GetServerSettings(id);
            settings.Status = true;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}`!");
        }

        [Command("disable")]
        public async Task DisableAlerts(string guildid)
        {
            var user = Context.User as SocketGuildUser;

            if (Context.User.Id != ConfigService.Config.AdminUser)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            ulong id = 99;
            try { id = Convert.ToUInt64(guildid); Context.Client.GetGuild(id); }
            catch { await ReplyAsync($"🔴 **Error:** Please provide a guild id!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id)); return; }

            var settings = ServerSettings.GetServerSettings(id);
            settings.Status = false;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Alerts will no longer be sent on the guild with id: `{guildid}`!");
        }

        [Command("send")]
        public Task SendAlert()
        {
            _ = Task.Run(async () =>
            {
                ConfigService.LoadConfig();
                var user = Context.User as SocketGuildUser;

                if (user.Id != ConfigService.Config.AdminUser)
                {
                    await ReplyAsync("Missing access!");
                    return;
                }

                await Context.Channel.SendMessageAsync("Confirm you would like to send an alert? `yes/no`");
                var confirm = await Context.NextMessageAsync();
                if (confirm.ToString().ToLower() == "yes")
                    await Context.Channel.SendMessageAsync("What should the title be?");
                else
                {
                    await Context.Channel.SendMessageAsync("You answered no or entered an invalid response; I canceled this alert!");
                    return;
                }
                var title = await Context.NextMessageAsync();

                await ReplyAsync("What should the description be?");
                var desc = await Context.NextMessageAsync();

                await Context.Channel.SendMessageAsync("Would you like to include an image? `yes/no`");
                bool withImage = false;
                var imageConfirm = await Context.NextMessageAsync();

                if (imageConfirm.ToString().ToLower() == "yes")
                {
                    await Context.Channel.SendMessageAsync("Please upload the image or send the url!");
                    withImage = true;
                }
                else if (imageConfirm.ToString().ToLower() == "no")
                {
                    await Context.Channel.SendMessageAsync("Would you like to send this alert? `yes/no`");
                    withImage = false;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("You answered no or entered an invalid response; I canceled this alert!");
                    return;
                }

                if (withImage)
                {
                    var image = await Context.NextMessageAsync();
                    string imageUrl = "";

                    if (image.Content == null)
                        imageUrl = image.Attachments.First().ProxyUrl;
                    else
                        imageUrl = image.Content;

                    await ReplyAsync("Great! Would you like to send this alert? `yes/no`");
                    var confirmSend = await Context.NextMessageAsync();

                    if (confirmSend.ToString().ToLower() == "yes")
                    {
                        await (Context.Channel as SocketTextChannel).SendAlertAsync(title.ToString(), desc.ToString(), imageUrl);
                        return;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("You answered no or entered an invalid response; I canceled this alert!");
                        return;
                    }
                }
                else
                {
                    var confirmSend = await Context.NextMessageAsync();
                    if (confirmSend.ToString().ToLower() == "yes")
                    {
                        await (Context.Channel as SocketTextChannel).SendAlertAsync(title.ToString(), desc.ToString());
                        return;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("You answered no or entered an invalid response; I canceled this alert!");
                        return;
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}