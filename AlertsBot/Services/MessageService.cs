using Discord.Commands;
using Discord.WebSocket;

namespace NextMessageAsync
{
    public class MessageService
    {
        private static List<(SocketMessage msg, TaskCompletionSource<SocketMessage> callbackSource)> Sources = new List<(SocketMessage msg, TaskCompletionSource<SocketMessage> callbackSource)>();
        private static DiscordSocketClient client;

        public MessageService(DiscordSocketClient c)
        {
            client = c;

            client.MessageReceived += CheckCallbacks;
        }

        private static async Task CheckCallbacks(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (Sources.Any(x => x.msg.Channel.Id == arg.Channel.Id && x.msg.Author.Id == arg.Author.Id))
            {
                var source = Sources.FirstOrDefault(x => x.msg.Channel.Id == arg.Channel.Id && x.msg.Author.Id == arg.Author.Id);

                source.callbackSource.SetResult(arg);
                Sources.Remove(source);
                return;
            }
        }

        public static Task<SocketMessage> NextMessageAsync(SocketMessage msg)
        {
            var completionSource = new TaskCompletionSource<SocketMessage>();

            Sources.Add((msg, completionSource));

            return completionSource.Task;
        }
    }

    public static class ContextExtension
    {
        public static async Task<SocketMessage> NextMessageAsync(this SocketCommandContext context)
        {
            return await MessageService.NextMessageAsync(context.Message);
        }
    }
}
