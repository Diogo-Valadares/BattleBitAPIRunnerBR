using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// Version: 1.1
/// </summary>
/// 
namespace BattlebitBRModules
{
    public class PlayerHudConfiguration : ModuleConfiguration
    {
        public bool KillFeedAllowed { get; set; } = true;
        public bool FriendlyHUDEnabled { get; set; } = true;
        public bool HitMarkersEnabled { get; set; } = true;
        public bool PointLogHudEnabled { get; set; } = true;
    }

    [RequireModule(typeof(CommandHandler))]
    public class HudCommands : BattleBitModule
    {
        public PlayerHudConfiguration HudConfig { get; set; }
        [ModuleReference]
        public CommandHandler CommandHandler { get; set; }

        public override void OnModulesLoaded()
        {
            this.CommandHandler.Register(this);
        }

        [CommandCallback("Hud", Description = "Ativa e desativa elementos de hud pro jogador", AllowedRoles = Roles.Admin)]
        public void Hud(RunnerPlayer commandSource,string hud, bool on)
        {
            switch (hud)
            {
                case "killfeed":
                case "kf":
                    Server.AnnounceShort("killfeed esta agora " + (on ? "permitido" : "bloqueado"));
                    HudConfig.KillFeedAllowed = on;
                    if (!on)
                    {
                        foreach(var player in Server.AllPlayers)
                        {
                            player.Modifications.KillFeed = false;
                        }
                    }
                    break;
                case "hitmarker":
                case "hm":
                    Server.AnnounceShort("hitmarker esta agora " + (on ? "ligada" : "desligada"));
                    HudConfig.HitMarkersEnabled = on;
                    break;
                case "friendlyhud":
                case "fh":
                    Server.AnnounceShort("Hud de aliados foi " + (on ? "ligada" : "desligada"));
                    HudConfig.FriendlyHUDEnabled = on;
                    break;
                case "pointlog":
                case "pl":
                    Server.AnnounceShort("Log de pontuação esta " + (on ? "ligado" : "desligado"));
                    HudConfig.FriendlyHUDEnabled = on;
                    break;
                case "all":
                    Server.AnnounceShort("Todas huds estão " + (on ? "ligadas" : "desligadas"));
                    HudConfig.FriendlyHUDEnabled = on;
                    HudConfig.HitMarkersEnabled = on;
                    HudConfig.KillFeedAllowed = on;
                    HudConfig.PointLogHudEnabled = on;
                    break;
                default:
                    Server.MessageToPlayer(commandSource, "Essa hud não existe");
                    return;
            }

            foreach (var player in Server.AllPlayers)
            {
                player.Modifications.KillFeed = HudConfig.KillFeedAllowed && player.Modifications.KillFeed;
                player.Modifications.HitMarkersEnabled = HudConfig.HitMarkersEnabled;
                player.Modifications.FriendlyHUDEnabled = HudConfig.FriendlyHUDEnabled;
                player.Modifications.PointLogHudEnabled = HudConfig.PointLogHudEnabled;
            }
        }

        [CommandCallback("KillFeed", Description = "permite o jogador individualmente ligar ou desligar o killfeed")]
        public void KillFeed(RunnerPlayer commandSource)
        {
            if (!HudConfig.KillFeedAllowed)
            {
                Server.MessageToPlayer(commandSource, "KillFeed is disabled on this server");
                return;
            }
            commandSource.Modifications.KillFeed = !commandSource.Modifications.KillFeed;

            Server.MessageToPlayer(commandSource, "KillFeed " + (commandSource.Modifications.KillFeed ? "ligado" : "desligado"));
        }


        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            player.Modifications.KillFeed = HudConfig.KillFeedAllowed && player.Modifications.KillFeed;
            player.Modifications.HitMarkersEnabled = HudConfig.HitMarkersEnabled;
            player.Modifications.FriendlyHUDEnabled = HudConfig.FriendlyHUDEnabled;
            player.Modifications.PointLogHudEnabled = HudConfig.PointLogHudEnabled;
            return Task.CompletedTask;
        }
    }
}