using Discord;
using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;

namespace AlertsBot.Handlers
{
    public class AllCommandHandler
    {
        public static DiscordSocketClient Client;

        private readonly InteractionService _interactions;

        public AllCommandHandler(DiscordSocketClient client, InteractionService interaction)
        {
            Client = client;
            _interactions = interaction;

            _interactions.SlashCommandExecuted += SlashCommandExecuted;
            Client.InteractionCreated += InteractionCreated;

            Client.Ready += OnReady;

            _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
        }
        
        private async Task OnReady()
        {
            await _interactions.RegisterCommandsGloballyAsync();
        }

        private async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Type: [{command.CommandType}] Command: [{command.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Interaction.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{Client.Latency}]");

                // special case for the 'RequirePermissions' precondition
                if (result.ErrorReason is "bad_perms")
                {
                    await context.Interaction.RespondAsync($"🔴 **Error:** You do not have access to use this command!", ephemeral: true);
                }
                
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
                Console.WriteLine($"Type: [{command.CommandType}] Command: [{command.Name}] User: [{context.User.Username}] Result: [{result}] Time: [{context.Interaction.CreatedAt.UtcDateTime.AddHours(-4)}] Latency: [{Client.Latency}]");
            }
        }

        private async Task InteractionCreated(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(Client, arg);
            await _interactions.ExecuteCommandAsync(context, null);
        }
    }
}