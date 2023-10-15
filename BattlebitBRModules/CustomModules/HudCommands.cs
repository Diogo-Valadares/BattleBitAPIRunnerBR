using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// </summary>
/// 
namespace BattleBitBaseModules;
[Module("Adds commands to activate or deactivate hud elements", "1.1")]
[RequireModule(typeof(CommandHandler))]
[RequireModule(typeof(PlayerStatsCache))]
public class HudCommands : BattleBitModule
{
    public PlayerHudConfiguration HudConfig { get; set; }
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }
    [ModuleReference]
    public PlayerStatsCache PlayerStatsCache { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("Hud", Description = "Ativa e desativa elementos de hud pro jogador", AllowedRoles = Roles.Admin)]
    public void Hud(RunnerPlayer commandSource, string hud, bool on)
    {
        switch (hud)
        {
            case "killfeed":
            case "kf":
                Server.AnnounceShort("killfeed esta agora " + (on ? "permitido" : "bloqueado"));
                HudConfig.KillFeedAllowed = on;
                if (!on)
                {
                    foreach (var player in Server.AllPlayers)
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
    public async void KillFeed(RunnerPlayer commandSource)
    {
        if (!HudConfig.KillFeedAllowed)
        {
            Server.MessageToPlayer(commandSource, "KillFeed esta desligado nesse server", 10); ;
            return;
        }
        if (PlayerStatsCache.Cache.TryGetValue(commandSource.SteamID, out var stats))
        {
            stats.Killfeed = !stats.Killfeed;
            if(!await PlayerStatsCache.UpdatePreference("Killfeed", stats.Killfeed ? 1 : 0, commandSource.SteamID))
            {
                Server.MessageToPlayer(commandSource, "Houve um erro a executar o comando, tente novamente", 10); ;
                return;
            }
            commandSource.Modifications.KillFeed = stats.Killfeed;

            Server.MessageToPlayer(commandSource, "Killfeed " + (commandSource.Modifications.KillFeed ? "ligado" : "desligado"),10);
        }
        else Server.MessageToPlayer(commandSource, "Houve um erro a executar o comando, tente novamente", 10);

    }
    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        Task.Delay(20);
        player.Modifications.HitMarkersEnabled = HudConfig.HitMarkersEnabled;
        player.Modifications.FriendlyHUDEnabled = HudConfig.FriendlyHUDEnabled;
        player.Modifications.PointLogHudEnabled = HudConfig.PointLogHudEnabled;
        if (PlayerStatsCache.Cache.TryGetValue(player.SteamID, out var stats))
            player.Modifications.KillFeed = HudConfig.KillFeedAllowed && stats == null || stats.Killfeed;
        return Task.CompletedTask;
    }

    public class PlayerHudConfiguration : ModuleConfiguration
    {
        public bool KillFeedAllowed { get; set; } = true;
        public bool FriendlyHUDEnabled { get; set; } = true;
        public bool HitMarkersEnabled { get; set; } = true;
        public bool PointLogHudEnabled { get; set; } = true;
    }
}