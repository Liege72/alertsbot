using Discord;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using System;

namespace AlertsBot.Handlers
{
    public class SlashCommandHandler
    {
        private readonly InteractionService _interactions;
        private readonly DiscordSocketClient _client;

        public SlashCommandHandler(DiscordSocketClient client, InteractionService interaction)
        {
            _client = client;
            _interactions = interaction;

            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            _client.InteractionCreated += InteractionCreated;

            _client.Ready += OnReady;

            _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        }

        public async Task OnReady()
        {
            await _interactions.RegisterCommandsGloballyAsync();
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