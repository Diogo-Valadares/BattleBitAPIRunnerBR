using BBRAPIModules;
using Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// </summary>

namespace BattleBitBaseModules;
[RequireModule(typeof(DatabaseComunicator))]
[Module("simple module that temporally stores player stats in a match. ", "1.0")]
public class PlayerStatsCache : BattleBitModule
{
    public ConcurrentDictionary<ulong, PlayerInfo> Cache { get; set; } = new();

    [ModuleReference]
    public DatabaseComunicator DatabaseComunicator { get; set; }
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }
    public override async Task OnConnected()
    {
        CommandHandler.Register(this);
        await Task.Delay(500);
        foreach (var player in Server.AllPlayers)
        {
            await OnPlayerConnected(player);
        }
    }
    public override async Task OnPlayerConnected(RunnerPlayer player)
    {
        if (Cache.ContainsKey(player.SteamID)) return;

        var playerDictionary = await DatabaseComunicator.GetPlayer(player.SteamID);
        if (playerDictionary.Count == 0)
        {
            Cache.Add(player.SteamID, new PlayerInfo(player));
        }
        else
        {
            var restrictions = playerDictionary.TryGetValue("Restrictions", out var r) ? (Dictionary<string, object>)r : null;
            var p = new PlayerInfo(player)
            {
                Deaths = 0,
                Kills = 0,
                Headshots = 0,
                Revives = 0,
                TimesRevived = 0,
                Points = 0,
                Killfeed = (ulong)playerDictionary["Killfeed"] > 0,
                BannedUntil = restrictions != null ? (restrictions.TryGetValue("ban", out var value) ? (DateTime)value : null) : null,
                MutedUntil = restrictions != null ? (restrictions.TryGetValue("mute", out var value2) ? (DateTime)value2 : null) : null,
                GaggedUntil = restrictions != null ? (restrictions.TryGetValue("gag", out var value3) ? (DateTime)value3 : null) : null
            };
            Cache.Add(player.SteamID, p);
        }
    }
    public override async Task OnRoundEnded()
    {
        await DumpCacheInDatabase();
    }
    public async Task<bool> UpdatePreference(string preference, int value, ulong player)
    {
        try
        {
            await DatabaseComunicator.UpdatePreference(preference, player, value);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /*public override async Task OnPlayerConnected(RunnerPlayer player)
   {
       if (DatabaseComunicator == null)
       {
           Console.WriteLine($"No Database Found");
           Cache.Add(player.SteamID, new PlayerInfo(player));
           return;
       }
       var playerDictionary = await DatabaseComunicator.GetPlayer(player.SteamID);

       if (playerDictionary.Count == 0)
       {
           Cache.Add(player.SteamID, new PlayerInfo(player));
           if (!await DatabaseComunicator.AddPlayer(player.SteamID, player.Name))
           {
               Console.WriteLine($"[Warning] PlayerStatsCache: failed to contact database while creating player {player.Name}");
           }
           else
           {
               Console.WriteLine($"PlayerStatsCache: added player({player.Name}) to database.");
           }
       }
       else
       {
           var restrictions = playerDictionary.TryGetValue("Restrictions", out var r) ? (Dictionary<string, object>)r : null;
           var p = new PlayerInfo(player)
           {
               Deaths = (uint)playerDictionary["Deaths"],
               Kills = (uint)playerDictionary["Kills"],
               Headshots = (uint)playerDictionary["Headshots"],
               Revives = (uint)playerDictionary["Revives"],
               TimesRevived = (uint)playerDictionary["TimesRevived"],
               Points = (uint)playerDictionary["Points"],
               Killfeed = (ulong)playerDictionary["Killfeed"] > 0,
               BannedUntil = restrictions != null ? (restrictions.TryGetValue("ban", out var value) ? (DateTime)value : null) : null,
               MutedUntil = restrictions != null ? (restrictions.TryGetValue("mute", out var value2) ? (DateTime)value2 : null) : null,
               GaggedUntil = restrictions != null ? (restrictions.TryGetValue("gag", out var value3) ? (DateTime)value3 : null) : null
           };
           Cache.Add(player.SteamID, p);
       }
   }
   public override async Task OnPlayerDisconnected(RunnerPlayer player)
   {
       if (!Cache.ContainsKey(player.SteamID)) return;

       if (DatabaseComunicator != null && !await DatabaseComunicator.UpdatePlayer(player.SteamID, ConvertPlayerInfo(Cache[player.SteamID])))
       {
           Console.WriteLine($"[Warning] PlayerStatsCache: failed to contact database while updating player {player.SteamID}");
       }
       Cache.Remove(player.SteamID);
   }*/

    /*[CommandCallback("PlayerStats", Description = "Shows the stats of a player", AllowedRoles = Roles.Moderator)]
    public void PlayerStats(RunnerPlayer commandSource, RunnerPlayer? player)
    {
        var target = player ?? commandSource;
        var output = "<size=20>";
        foreach (var playerProperty in ConvertPlayerInfo(target))
        {
            if (playerProperty.Key == "IP") continue;
            if (playerProperty.Key == "Player")
            {
                output += "SteamID:" + ((RunnerPlayer)playerProperty.Value).SteamID + "\n";
                output += "Name:" + ((RunnerPlayer)playerProperty.Value).Name + "\n";
            }
            else
            {
                output += $"{playerProperty.Key}: {playerProperty.Value}\n";
            }
        }
        Server.WarnPlayer(commandSource.SteamID, output);
    }*/

    public class PlayerInfo
    {
        public RunnerPlayer player { get; set; }
        public uint Kills { get; set; } = 0;
        public uint Deaths { get; set; } = 0;
        public uint Revives { get; set; } = 0;
        public uint TimesRevived { get; set; } = 0;
        public uint Points { get; set; } = 0; //implement once oki releases an update to track this
        public uint Headshots { get; set; } = 0;
        public bool Killfeed { get; set; } = true;
        public DateTime? BannedUntil { get; set; } = null;
        public DateTime? GaggedUntil { get; set; } = null;
        public DateTime? MutedUntil { get; set; } = null;
        public float KDR { get { return (float)Kills / Deaths; } }
        public PlayerInfo() { }
        public PlayerInfo(RunnerPlayer player)
        {
            this.player = player;
        }
    }

    private async Task DumpCacheInDatabase()
    {
        var dump = new List<Dictionary<string, object>>();
        var currentPlayers = Server.AllPlayers.ToDictionary(p => p.SteamID);
        foreach (var player in Cache)
        {
            dump.Add(ConvertPlayerInfo(player.Value));
        }
        if (await DatabaseComunicator.UpdatePlayers(dump))
        {
            foreach (var player in Cache)
            {                
                player.Value.Headshots = 0;
                player.Value.TimesRevived = 0;
                player.Value.Points = 0;
                player.Value.Kills = 0;
                player.Value.Revives = 0;
                player.Value.Deaths = 0;
                if (currentPlayers.ContainsKey(player.Key)) continue;
                Cache.Remove(player.Key);
            }
        }
    }

    private static Dictionary<string, object> ConvertPlayerInfo<T>(T info) where T : new()
    {
        return info.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(info, null) ?? "");
    }
    private static T ConvertDictionaryTo<T>(IDictionary<string, object> dictionary) where T : new()
    {
        Type type = typeof(T);
        T ret = new();

        foreach (var keyValue in dictionary)
        {
            type.GetProperty(keyValue.Key)?.SetValue(ret, keyValue.Value, null);
        }

        return ret;
    }
}