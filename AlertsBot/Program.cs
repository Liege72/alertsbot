using Discord;
using Discord.WebSocket;
using Discord.Commands;
using AlertsBot.Services;
using NextMessageAsync;

namespace AlertsBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient Client;
        public async Task MainAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose, MessageCacheSize = 10 });
            var service = new CommandService();
            var messageService = new MessageService(Client);
            var handler = new CommandHandler(service, Client);

            Client.Log += LogAsync;
            service.Log += LogAsync;

            await Client.LoginAsync(TokenType.Bot, ConfigService.Config.BotToken);
            await Client.StartAsync();

            if (ConfigService.Config.CustomStatus != null)
                await Client.SetGameAsync(ConfigService.Config.CustomStatus);

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}