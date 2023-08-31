using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

namespace BattlebitBRModules
{
    public class ConfigurationKillFeed : ModuleConfiguration
    {
        public bool KillFeed { get; set; } = true;
        public bool KillFeedAllowed { get; set; } = true;
    }

    [RequireModule(typeof(CommandHandler))]
    public class KillFeedCommands : BattleBitModule
    {
        public ConfigurationKillFeed ServerKillFeedConfig { get; set; }
        [ModuleReference]
        public CommandHandler commandHandler { get; set; }

        public override void OnModulesLoaded()
        {
            commandHandler.Register(this);
        }

        [CommandCallback("ServerKillFeed", Description = "blocks or allows killFeed", AllowedRoles = Roles.Admin)]
        public void ServerKillFeed(RunnerPlayer commandSource, bool on)
        {
            Server.AnnounceShort("killfeed is now " + (on ? "allowed" : "blocked"));
            ServerKillFeedConfig.KillFeedAllowed = on;
        }

        [CommandCallback("KillFeed", Description = "Allows any player to turn killfeed on or off")]
        public void KillFeed(RunnerPlayer commandSource, bool on)
        {
            if (!ServerKillFeedConfig.KillFeed)
            {
                Server.MessageToPlayer(commandSource.SteamID, "KillFeed is disabled on this server");
                return;
            }
            commandSource.Modifications.KillFeed = on;
        }


        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            player.Modifications.KillFeed = ServerKillFeedConfig.KillFeedAllowed;
            return Task.CompletedTask;
        }
    }
}