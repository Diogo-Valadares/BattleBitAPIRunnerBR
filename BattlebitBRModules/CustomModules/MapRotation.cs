using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami modified by @_dx2
/// Version: 1.3
/// </summary>

public class MapRotation : BattleBitModule
{
    private readonly string[] mapsNames = new[]
    {
        "Azagor",
        "Basra",
        "Construction",
        "District",
        "Dustydew",
        "Eduardovo",
        "Frugis",
        "Isle",
        "Lonovo",
        "MultuIslands",
        "Namak",
        "OilDunes",
        "River",
        "Salhan",
        "SandySunset",
        "TensaTown",
        "Valley",
        "Wakistan",
        "WineParadise",
        "Old_Namak",
        "Old_District",
        "Old_OilDunes",
        "Old_Eduardovo",
        "Old_MultuIslands"
    };

    public MapRotationConfiguration Configuration { get; set; }

    public override Task OnConnected()
    {
        this.Server.MapRotation.SetRotation(this.Configuration.Maps);
        return Task.CompletedTask;
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.CountingDown)
        {
            if (Configuration.MatchesSinceSelection.Length != Configuration.Maps.Length)
            {
                ReinicializeCounters();
            }
            var currentMapIndex = Array.IndexOf(Configuration.Maps, Server.Map);
            if (currentMapIndex == -1)
            {
                Console.WriteLine($"Current map({Server.Map}) not found in MapRotation ConfigList while reseting the counter(Did you type the name correctly?)");
            }
            else
            {
                Configuration.MatchesSinceSelection[currentMapIndex] = 0;
            }

            var mapsThisRound = this.Configuration.Maps.Zip(Configuration.MatchesSinceSelection)
                .OrderByDescending(map => map.Second).ToList()
                .GetRange(0, this.Configuration.MapCountInRotation).ConvertAll(m => m.First).ToArray();

            this.Server.MapRotation.SetRotation(mapsThisRound);
            for (int i = 0; i < Configuration.MatchesSinceSelection.Length; i++)
            {
                Configuration.MatchesSinceSelection[i]++;
            }
        }
        return Task.CompletedTask;
    }

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
        Configuration.Load();
        for (int i = Configuration.Maps.Length - 1; i >= 0; i--)
        {
            var correctName = FindMapName(Configuration.Maps[i]);
            if (correctName == null)
            {
                Configuration.Maps = Configuration.Maps.Except(new string[] { Configuration.Maps[i] }).ToArray();
            }
            else
            {
                Configuration.Maps[i] = correctName;
            }
        }
        if (Configuration.MatchesSinceSelection.Length != Configuration.Maps.Length)
        {
            ReinicializeCounters();
        }
    }

    [CommandCallback("Maps", Description = "Mostra a rotação atual")]
    public void Maps(RunnerPlayer commandSource)
    {
        string maps = "";
        foreach (var map in Configuration.Maps)
        {
            maps += map + ", ";
        }
        Server.MessageToPlayer(commandSource, $"A rotação de mapas atual é: {maps}");
    }

    [CommandCallback("AddMap", Description = "Adds a map in the current rotation", AllowedRoles = Roles.Admin)]
    public void AddMap(RunnerPlayer commandSource, string map)
    {
        var matchingName = FindMapName(commandSource, map);
        if (matchingName == null) return;

        var mapIndex = Array.IndexOf(Configuration.Maps, matchingName);
        if (mapIndex != -1)
        {
            Server.SayToChat($"{matchingName} is already in rotation", commandSource);
            return;
        }
        Configuration.Maps = Configuration.Maps.Append(matchingName).ToArray();
        Configuration.Save();

        Server.SayToChat($"Successfuly added {matchingName} to rotation", commandSource);
    }

    [CommandCallback("RemoveMap", Description = "Removes a map from the current rotation", AllowedRoles = Roles.Admin)]
    public void RemoveMap(RunnerPlayer commandSource, string map)
    {
        var matchingName = FindMapName(commandSource, map);
        if (matchingName == null) return;

        var mapIndex = Array.IndexOf(Configuration.Maps, map);
        if (mapIndex == -1)
        {
            Server.SayToChat($"{matchingName} is already off rotation or doesn't exist", commandSource);
            return;
        }
        Configuration.Maps = Configuration.Maps.Except(new string[] { matchingName }).ToArray();
        Configuration.Save();
    }

    private string? FindMapName(string mapName)
    {
        var matchingNames = Array.FindAll(mapsNames, m => m.ToLower().StartsWith(mapName.ToLower()));
        if (!matchingNames.Any())
        {
            Console.WriteLine($"{mapName} does not exist, removing from list.");
            return null;
        }
        if (matchingNames.Length > 1)
        {
            Console.WriteLine($"Multiple maps starts with {mapName}, removing from list.");
            return null;
        }
        return matchingNames[0];
    }
    private string? FindMapName(RunnerPlayer commandSource, string mapName)
    {
        var matchingNames = Array.FindAll(mapsNames, m => m.ToLower().StartsWith(mapName.ToLower()));
        if (!matchingNames.Any())
        {
            Server.SayToChat($"{mapName} does not exist, check your typing.", commandSource);
            return null;
        }
        if (matchingNames.Length > 1)
        {
            Server.SayToChat($"Multiple maps starts with {mapName}, please try again.", commandSource);
            return null;
        }
        return matchingNames[0];
    }

    private void ReinicializeCounters()
    {
        Configuration.MatchesSinceSelection = new int[Configuration.Maps.Length];
        Random r = new();
        for (int i = 0; i < Configuration.Maps.Length; i++)
        {
            Configuration.MatchesSinceSelection[i] = r.Next(5);
        }
        Configuration.Save();
    }

    /*//use for debugging
    [CommandCallback("M", Description = "Shows how many matches since the last time a map was played")]
    public void M(RunnerPlayer commandSource)
    {
        string maps = "";
        int i = 0;
        foreach (var map in Configuration.Maps)
        {
            maps += map + " " + matchesSinceSelection[i] + ", ";
            i++;
        }
        Server.SayToChat($"maps played and times since last played: {maps}", commandSource);
    }
    [CommandCallback("CM", Description = "Shows the Current Map name returned by Server.map")]
    public void CM(RunnerPlayer commandSource)
    {
        Server.MessageToPlayer(commandSource, $"Current map {Server.Map}");
    }*/

}

public class MapRotationConfiguration : ModuleConfiguration
{
    public int MapCountInRotation { get; set; } = 8;
    public int[] MatchesSinceSelection { get; set; } = new int[1];
    public string[] Maps { get; set; } = new[]
    {
        "Azagor",
        "Basra",
        "Construction",
        "District",
        "Dustydew",
        "Eduardovo",
        "Frugis",
        "Isle",
        "Lonovo",
        "MultuIslands",
        "Namak",
        "OilDunes",
        "River",
        "Salhan",
        "SandySunset",
        "TensaTown",
        "Valley",
        "Wakistan",
        "WineParadise",
        "Old_Namak",
        "Old_District",
        "Old_OilDunes",
        "Old_Eduardovo",
        "Old_MultuIslands"
    };
}