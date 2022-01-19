using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace AlertsBot.Services
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        public CommandHandler(CommandService service, DiscordSocketClient client)
        {
            _commands = service;
            _client = client;

            _commands.CommandExecuted += CommandExecutedAsync;
            _client.MessageReceived += MessageReceivedAsync;

            _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            var argPos = 0;
            string prefix = ConfigService.Config.Prefix;

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {

            if (!command.IsSpecified)
            {
                Console.WriteLine("Unknown Command Was Used");
                return;
            }

            if (result.IsSuccess)
            {
                Console.WriteLine($"Success | Command: [{command.Value.Name}] User: [{context.User.Username}] Time: [{DateTime.UtcNow}]");
                return;
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor("Command Error");
                embed.WithDescription("Looks like there was an error, check your input and try again.");
                embed.WithColor(Color.Red);
                await context.Channel.SendMessageAsync("", false, embed.Build());

                Console.WriteLine($"Error | Command: [{command.Value.Name}] User: [{context.User.Username}] Time: [{DateTime.UtcNow}] Result: [{result.ErrorReason}]");
                return;
            }
        }
    }
}