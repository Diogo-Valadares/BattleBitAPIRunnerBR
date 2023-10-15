using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// Version: 1.0
/// </summary>
/// 
namespace BattleBitBaseModules;

public class PlayerGiveUpModule : BattleBitModule
{
    private readonly HashSet<ulong> GiveUpsTeamA = new();
    private readonly HashSet<ulong> GiveUpsTeamB = new();

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    [CommandCallback("Desistir", Description = "Adiciona um voto para desistir da partida")]
    public void Desistir(RunnerPlayer commandSource)
    {
        if(commandSource.Team == Team.TeamA)
        {
            if (GiveUpsTeamA.Contains(commandSource.SteamID))
            {
                Server.SayToChat("<color=red>Você já votou", commandSource.SteamID);
            }
            GiveUpsTeamA.Add(commandSource.SteamID);
            if (GiveUpsTeamA.Count / Server.AllTeamAPlayers.Count() > 0.5f)
            {
                Server.ExecuteCommand("forceend");
            }
        }
        else if (commandSource.Team == Team.TeamB)
        {
            if (GiveUpsTeamB.Contains(commandSource.SteamID))
            {
                Server.SayToChat("<color=red>Você já votou", commandSource.SteamID);
            }
            GiveUpsTeamB.Add(commandSource.SteamID);
            if (GiveUpsTeamB.Count / Server.AllTeamBPlayers.Count() > 0.5f)
            {
                Server.ExecuteCommand("forceend");
            }
        }
        else
        {
            Server.SayToChat("<color=red>Você não esta em nenhum time", commandSource.SteamID);
        }
    }

    public override Task<bool> OnPlayerRequestingToChangeTeam(RunnerPlayer player, Team requestedTeam)
    {
        if (requestedTeam == Team.TeamA)
        {
            if(GiveUpsTeamB.Contains(player.SteamID))
            {
                GiveUpsTeamB.Remove(player.SteamID);
            }
        }
        else
        {
            if (GiveUpsTeamA.Contains(player.SteamID))
            {
                GiveUpsTeamA.Remove(player.SteamID);
            }
        }
        return Task.FromResult(true);
    }

    public override Task OnRoundEnded()
    {
        GiveUpsTeamA.Clear();
        GiveUpsTeamB.Clear();
        return Task.CompletedTask;
    }
}
