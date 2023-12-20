using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// Version: 1.0
/// </summary>
/// 
namespace BattleBitBaseModules;

public class PlayerBalance : BattleBitModule
{
    public PlayerBalanceConfig Config { get; set; }

    public override Task<bool> OnPlayerRequestingToChangeTeam(RunnerPlayer player, Team requestedTeam)
    {
        if(Math.Abs(Server.AllTeamAPlayers.Count() - Server.AllTeamBPlayers.Count()) > Config.MaxPlayerDifference)
        {
            Server.MessageToPlayer(player,"<color=red>O time inimigo possui muitos inimigos para você poder trocar");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public override Task OnPlayerDisconnected(RunnerPlayer player)
    {
        if (Math.Abs(Server.AllTeamAPlayers.Count() - Server.AllTeamBPlayers.Count()) > Config.MaxPlayerDifference)
        {
            if(player.Team == Team.TeamA)
            {
                Server.AllTeamBPlayers.MinBy(player => player.)
            }
        }
    }
}

public class PlayerBalanceConfig : ModuleConfiguration
{
    public int MaxPlayerDifference { get; set; } = 3;
}