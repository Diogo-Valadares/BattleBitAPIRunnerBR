using BattleBitAPI.Common;
using BBRAPIModules;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// </summary>
/// 
namespace BattleBitBaseModules;

[Module("Used to track the player stats and saving in a player stats cache", "1.0")]
[RequireModule(typeof(PlayerStatsCache))]
public class PlayerStatsTracker : BattleBitModule
{
    [ModuleReference]
    public PlayerStatsCache PlayerStatsCache { get; set; }

    public override Task OnAPlayerRevivedAnotherPlayer(RunnerPlayer reviver, RunnerPlayer revived)
    {
        if (PlayerStatsCache.Cache.TryGetValue(reviver.SteamID, out var stats))
        {
            stats.Revives++;
        }
        if (PlayerStatsCache.Cache.TryGetValue(revived.SteamID, out var stats2))
        {
            stats2.TimesRevived++;
        }

        return Task.CompletedTask;
    }
    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<RunnerPlayer> args)
    {
        if (args.Killer == args.Victim) return Task.CompletedTask;
        if (PlayerStatsCache.Cache.TryGetValue(args.Killer.SteamID, out var stats))
        {
            if (args.BodyPart == PlayerBody.Head)
            {
                stats.Headshots++;
            }
            stats.Kills++;
        }
        return Task.CompletedTask;
    }
    public override Task OnPlayerDied(RunnerPlayer player)
    {
        if (PlayerStatsCache.Cache.TryGetValue(player.SteamID, out var stats))
        {
            stats.Deaths++;
        }
        return Task.CompletedTask;
    }

    public override Task OnRoundStarted()
    {
        foreach (var player in Server.AllPlayers)
        {
            if (!PlayerStatsCache.Cache.ContainsKey(player.SteamID))
            {
                PlayerStatsCache.Cache.Add(player.SteamID, new PlayerStatsCache.PlayerInfo(player));
            }
        }
        var serverPlayers = Server.AllPlayers.ToHashSet();
        foreach (var player in PlayerStatsCache.Cache)
        {
            if (!serverPlayers.Contains(player.Value.player))
            {
                PlayerStatsCache.Cache.Remove(player.Key);
            }
        }
        return Task.CompletedTask;
    }
}