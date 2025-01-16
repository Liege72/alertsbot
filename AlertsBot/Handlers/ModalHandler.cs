using AlertsBot.Services;
using Discord.WebSocket;

namespace AlertsBot.Handlers
{
    public class ModalHandler
    {
        private readonly DiscordSocketClient? _client;

        public ModalHandler(DiscordSocketClient? client)
        {
            _client = client;

            _client.ModalSubmitted += ModalSubmitted;
        }

        private static async Task ModalSubmitted(SocketModal arg)
        {
            ConfigService.LoadConfig();

            var id = arg.Data.CustomId;
            var comps = arg.Data.Components;

            if (id == "alert")
            {
                var alertTitle = comps.First(x => x.CustomId == "alert_title").Value;
                var alertText = comps.First(x => x.CustomId == "alert_content").Value;
                string? alertImageUrl = null;
                string? alertFooter = null;

                if (comps.FirstOrDefault(x => x.CustomId == "alert_image")?.Value != null)
                    alertImageUrl = comps.FirstOrDefault(x => x.CustomId == "alert_image")?.Value;

                if (comps.FirstOrDefault(x => x.CustomId == "alert_footer")?.Value != null)
                    alertFooter = comps.FirstOrDefault(x => x.CustomId == "alert_footer")?.Value;

                await arg.SendAlertAsync(alertTitle, alertText, alertImageUrl!, alertFooter!);          
            }
        }
    }
}
