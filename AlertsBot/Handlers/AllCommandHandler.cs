using Discord;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using Discord.Commands;
using AlertsBot.Services;

namespace AlertsBot.Handlers
{
    public class AllCommandHandler
    {
        public readonly InteractionService _interactions;
        public static DiscordSocketClient _client;
        public readonly CommandService _commands;

        public AllCommandHandler(DiscordSocketClient client, InteractionService interaction, CommandService commands)
        {
            _client = client;
            _interactions = interaction;
            _commands = commands;

            _client.MessageReceived += MessageReceivedAsync;
            _commands.CommandExecuted += CommandExecutedAsync;

            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _client.InteractionCreated += InteractionCreated;

            _client.Ready += OnReady;

            _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
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

            ConfigService.LoadConfig();

            var argPos = 0;
            char prefix = ConfigService.Config.Prefix;

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);
        }

        public async Task CommandExecutedAsync(Discord.Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
        {
            if (!command.IsSpecified)
            {
                Console.WriteLine("Unknown Command Was Used");
                return;
            }

            if (result.IsSuccess)
            {
                Console.WriteLine($"Type: [Text] Command: [{command.Value.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Message.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{_client.Latency}]");
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor("Command Error", "https://cdn.discordapp.com/emojis/312314733816709120.png?v=1");
                embed.WithDescription("Hey there! Looks like there was an error, check if you used the command correctly.");
                embed.WithColor(Color.Red);
                await context.Channel.SendMessageAsync("", false, embed.Build());

                Console.WriteLine($"Type: [Text] Command: [{command.Value.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Message.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{_client.Latency}]");
            }
        }

        public async Task OnReady()
        {
            ConfigService.LoadConfig();
            if (ConfigService.Config.slashEnabled)
                await _interactions.RegisterCommandsGloballyAsync();
            else
                return;
        }

        public async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Type: [{command.CommandType}] Command: [{command.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Interaction.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{_client.Latency}]");

                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.RespondAsync($"Command Error: unmet precondition (5)", ephemeral: true);
                        break;
                    case InteractionCommandError.UnknownCommand:
                        await context.Interaction.RespondAsync("Unknown command (0)", ephemeral: true);
                        break;
                    case InteractionCommandError.BadArgs:
                        await context.Interaction.RespondAsync("Command Error: bad args (2)", ephemeral: true);
                        break;
                    case InteractionCommandError.Exception:
                        await context.Interaction.RespondAsync($"There was an error, check your input and try again! (3)", ephemeral: true);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        await context.Interaction.RespondAsync("Command Error: unsuccessful (4)", ephemeral: true);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Type: [{command.CommandType}] Command: [{command.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Interaction.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{_client.Latency}]");
            }
        }

        public async Task InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(_client, arg);
            await _interactions.ExecuteCommandAsync(context, null);
        }
    }
}