using Discord;
using Discord.WebSocket;
using Discord.Commands;
using AlertsBot.Services;
using NextMessageAsync;
using AlertsBot.Handlers;
using Discord.Interactions;

namespace AlertsBot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient client;
        public static InteractionService service;
        public async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig 
            { 
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.DirectMessages
            });
            var service = new InteractionService(client);
            var handler = new SlashCommandHandler(client, service);

            client.Log += LogAsync;
            service.Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, ConfigService.Config.BotToken);
            await client.StartAsync();

            if (ConfigService.Config.CustomStatus != null)
                await client.SetGameAsync(ConfigService.Config.CustomStatus);

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}