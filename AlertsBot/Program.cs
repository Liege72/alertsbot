using Discord;
using Discord.WebSocket;
using AlertsBot.Services;
using AlertsBot.Handlers;
using Discord.Interactions;
using Discord.Commands;
using NextMessageAsync;

namespace AlertsBot
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            ConfigService.LoadConfig();

            var client = new DiscordSocketClient(new DiscordSocketConfig 
            { 
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.DirectMessages | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers,
                MessageCacheSize = 20
            });
            var interactions = new InteractionService(client);
            var commands = new CommandService();

            var handler = new AllCommandHandler(client, interactions, commands);
            var modalHandler = new ModalHandler(client);
            var messageService = new MessageService(client);

            client.Log += LogAsync;
            interactions.Log += LogAsync;
            commands.Log += LogAsync;

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