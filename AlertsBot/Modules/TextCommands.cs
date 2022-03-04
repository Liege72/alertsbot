using AlertsBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using NextMessageAsync;
using System.Threading.Tasks;
using static AlertsBot.Services.DatabaseService;

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
        public async Task EnableAlerts(ulong guildid)
        {
            var user = Context.User as SocketGuildUser;

            if (Context.User.Id != ConfigService.Config.AdminUser)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            if (guildid.ToString() == null)
            {
                await ReplyAsync($"🔴 **Error:** Please provide a guild id!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            var settings = ServerSettings.GetServerSettings(guildid);
            settings.Status = true;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Alerts will now be sent on the guild with id: `{guildid}`!");
        }

        [Command("disable")]
        public async Task DisableAlerts(ulong guildid)
        {
            var user = Context.User as SocketGuildUser;

            if (Context.User.Id != ConfigService.Config.AdminUser)
            {
                await ReplyAsync($"🔴 **Error:** You do not have access to use this command!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            if (guildid.ToString() == null)
            {
                await ReplyAsync($"🔴 **Error:** Please provide a guild id!", messageReference: new MessageReference(Context.Message.Id, Context.Message.Channel.Id, user.Guild.Id));
                return;
            }

            var settings = ServerSettings.GetServerSettings(guildid);
            settings.Status = false;
            settings.SaveThis();
            await Context.Channel.SendMessageAsync($"🟢 **Success:** Alerts will no longer be sent on the guild with id: `{guildid}`!");
        }

        static MongoClient Client = new MongoClient(ConfigService.Config.MongoCS);

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

                switch (confirm.ToString().ToLower())
                {
                    case "yes":
                        await Context.Channel.SendMessageAsync("Got it! Lets continue...");
                        break;

                    case "no":
                        await Context.Channel.SendMessageAsync("Got it! I will cancel this alert.");
                        return;

                    default:
                        await Context.Channel.SendMessageAsync("Invalid response! Cancelled alert.");
                        return;
                }

                var embed = new EmbedBuilder();
                embed.WithCurrentTimestamp();
                embed.WithColor(new Color(48, 52, 52));

                await ReplyAsync("What should the title be?");
                var title = await Context.NextMessageAsync();

                if (title.ToString().ToLower().Equals("cancel"))
                {
                    await ReplyAsync("Cancelled alert!");
                    return;
                }

                embed.WithTitle(title.ToString());
                await ReplyAsync("This is what your alert looks like so far...", false, embed.Build());

                await ReplyAsync("What should the description be?");
                var desc = await Context.NextMessageAsync();

                if (desc.ToString().ToLower().Equals("cancel"))
                {
                    await ReplyAsync("Cancelled alert!");
                    return;
                }

                embed.WithDescription(desc.Content);
                await ReplyAsync("This is what your alert looks like so far...", false, embed.Build());

                await ReplyAsync("Would you like to include an image? `yes/no`");
                var imageConfirm = await Context.NextMessageAsync();
                bool imageCheck;
                switch (imageConfirm.ToString().ToLower())
                {
                    case "yes":
                        await ReplyAsync("Got it! Please send the image link.");
                        imageCheck = true;
                        break;

                    case "no":
                        await ReplyAsync("Got it! Lets continue!");
                        imageCheck = false;
                        break;

                    default:
                        await ReplyAsync("Invalid response! Cancelled alert.");
                        return;
                }

                if (imageCheck == true)
                {
                    var image = await Context.NextMessageAsync();
                    embed.WithImageUrl(image.ToString());
                    await ReplyAsync("", false, embed.Build());

                    await ReplyAsync("Would you like to send this alert? `yes/no`");
                    var send = await Context.NextMessageAsync();

                    switch (send.ToString().ToLower())
                    {
                        case "yes":
                            Embed[] embedArray = new Embed[] { embed.Build() };

                            var database = Client.GetDatabase(ConfigService.Config.DatabaseName);
                            var collection = database.GetCollection<BsonDocument>(ConfigService.Config.CollectionName);

                            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();

                            if (documents.Count == 0)
                            {
                                await ReplyAsync("There are no servers to send to!");
                                return;
                            }

                            foreach (var doc in documents)
                            {
                                long guildid1 = doc["GuildID"].AsInt64;
                                long channelid1 = doc["ChannelID"].AsInt64;
                                long roleid1 = doc["RoleID"].AsInt64;
                                long userid1 = doc["UserID"].AsInt64;
                                bool status = doc["Status"].AsBoolean;

                                try
                                {
                                    if (status == false)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        var guildid = (ulong)guildid1;
                                        var channelid = (ulong)channelid1;
                                        var userid = (ulong)userid1;

                                        var _client = Context.Client;

                                        SocketGuild guild = _client.GetGuild(guildid);
                                        SocketTextChannel channel1 = guild.GetTextChannel(channelid);
                                        var botUser = _client.GetGuild(guildid).GetUser(_client.CurrentUser.Id);

                                        if (!botUser.GetPermissions(channel1).ViewChannel)
                                        {
                                            var userAccount = _client.GetUser(userid);
                                            await userAccount.SendMessageAsync($"I am missing access to the {channel1.Name} channel, please give me access to view this channel so I can send alerts!");
                                            continue;
                                        }

                                        if (!botUser.GetPermissions(channel1).SendMessages)
                                        {
                                            var userAccount = _client.GetUser(userid);
                                            await userAccount.SendMessageAsync($"I am missing access to the {channel1.Name} channel, please give me access to send messages to this channel so I can send alerts!");
                                            continue;
                                        }

                                        if (roleid1 == 0)
                                        {
                                            await channel1.SendMessageAsync("", false, embed.Build());
                                        }
                                        else
                                        {
                                            await channel1.SendMessageAsync($"<@&{roleid1}>", false, embed.Build());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }

                            await ReplyAsync("Sent to all servers possible!");
                            return; ;

                        case "no":
                            await ReplyAsync("Got it! I will not send the alert.");
                            return; ;

                        default:
                            await ReplyAsync("Invalid response! Cancelled alert.");
                            return;
                    }
                }
                else
                {
                    await ReplyAsync("", false, embed.Build());

                    await ReplyAsync("Would you like to send this alert? `yes/no`");
                    var send = await Context.NextMessageAsync();

                    switch (send.ToString().ToLower())
                    {
                        case "yes":

                            Embed[] embedArray = new Embed[] { embed.Build() };

                            var database = Client.GetDatabase(ConfigService.Config.DatabaseName);
                            var collection = database.GetCollection<BsonDocument>(ConfigService.Config.CollectionName);

                            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();

                            if (documents.Count == 0)
                            {
                                await ReplyAsync("There are no servers to send to!");
                                return;
                            }

                            foreach (var doc in documents)
                            {
                                long guildid1 = doc["GuildID"].AsInt64;
                                long channelid1 = doc["ChannelID"].AsInt64;
                                long roleid1 = doc["RoleID"].AsInt64;
                                long userid1 = doc["UserID"].AsInt64;
                                bool status = doc["Status"].AsBoolean;

                                try
                                {
                                    if (status == false)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        var guildid = (ulong)guildid1;
                                        var channelid = (ulong)channelid1;
                                        var userid = (ulong)userid1;

                                        var _client = Context.Client;

                                        SocketGuild guild = _client.GetGuild(guildid);
                                        SocketTextChannel channel1 = guild.GetTextChannel(channelid);

                                        var botUser = _client.GetGuild(guildid).GetUser(_client.CurrentUser.Id);

                                        if (!botUser.GetPermissions(channel1).ViewChannel)
                                        {
                                            var userAccount = _client.GetUser(userid);
                                            await userAccount.SendMessageAsync($"I am missing access to the {channel1.Name} channel, please give me access to view this channel so I can send alerts!");
                                            continue;
                                        }

                                        if (!botUser.GetPermissions(channel1).SendMessages)
                                        {
                                            var userAccount = _client.GetUser(userid);
                                            await userAccount.SendMessageAsync($"I am missing access to the {channel1.Name} channel, please give me access to send messages to this channel so I can send alerts!");
                                            continue;
                                        }

                                        if (roleid1 == 0)
                                        {
                                            await channel1.SendMessageAsync("", false, embed.Build());
                                        }
                                        else
                                        {
                                            await channel1.SendMessageAsync($"<@&{roleid1}>", false, embed.Build());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            await ReplyAsync("Sent to all servers possible!");

                            return;

                        case "no":
                            await ReplyAsync("Got it! I will not send the alert.");
                            return;

                        default:
                            await ReplyAsync("Invalid response! Cancelled alert.");
                            return;
                    }
                }
            });
            return Task.CompletedTask;
        }
    }
}