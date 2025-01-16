using AlertsBot.Services;
using Discord.Commands;
using Discord.WebSocket;

namespace AlertsBot
{
    public class RequirePermissions : PreconditionAttribute
    {
        /// <summary>
        /// A precondition that requires the context user to be listed as an admin in the config file OR an administrator on the server
        /// </summary>
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (context.User is not SocketGuildUser user)
                return await Task.FromResult(
                    PreconditionResult.FromError("This action cannot be done outside of a guild!"));
            ConfigService.LoadConfig();

            if (ConfigService.Config.AdminUsers.Any(x => x == user.Id.ToString()) ||
                user.GuildPermissions.Administrator)
                return await Task.FromResult(PreconditionResult.FromSuccess());
            return await Task.FromResult(PreconditionResult.FromError("bad_perms"));
        }
    }

    public class RequireConfigUser : PreconditionAttribute
    {
        /// <summary>
        /// A precondition that requires the context user to be listed as an admin in the config file
        /// </summary>
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            if (context.User is not SocketGuildUser user)
                return await Task.FromResult(
                    PreconditionResult.FromError("This action cannot be done outside of a guild!"));
            ConfigService.LoadConfig();

            if (ConfigService.Config.AdminUsers.Any(x => x == user.Id.ToString()))
                return await Task.FromResult(PreconditionResult.FromSuccess());
            return await Task.FromResult(PreconditionResult.FromError("bad_perms"));
        }
    }
}
