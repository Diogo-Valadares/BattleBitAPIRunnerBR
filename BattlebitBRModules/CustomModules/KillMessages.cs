using BattleBitAPI.Common;
using BattleBitBaseModules;
using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2 incremented by Goim
/// Version: 1.0
/// </summary>
/// 
namespace BattlebitBRModules;

public class KillMessages : BattleBitModule
{
    [ModuleReference]
    public RichText RichText { get; set; }

    private readonly List<ulong> c4SuicideKillerIds = new();
    private readonly List<ulong> explosiveKills = new();

    private readonly Dictionary<ulong, int> killstreak = new();

    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<RunnerPlayer> onPlayerKill)
    {
        string white = RichText?.FromColorName("white") ?? "";
        string blue = RichText?.Color("#6688FF") ?? "";
        string red = RichText?.Color("#FF1B1B") ?? "";
        string green = RichText?.FromColorName("green") ?? "";
        string gold = RichText?.Color("#FFBF00") ?? "";
        string magenta = RichText?.Color("#FF00FF") ?? "";
        switch (onPlayerKill.KillerTool)
        {
            case "Sledge Hammer":
                Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}esmagou o {blue}crânio {white}de {red}{onPlayerKill.Victim.Name} {white}como uma melancia.");
                break;
            case "Pickaxe":
                Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}passou a {blue}lambida {white}em {red}{onPlayerKill.Victim.Name}.");
                break;
            case "SuicideC4":
                if (onPlayerKill.Killer == onPlayerKill.Victim)
                {
                    await Task.Delay(200);
                    var kills = c4SuicideKillerIds.FindAll(suicider => suicider == onPlayerKill.Killer.SteamID).Count;
                    if (kills == 0)
                    {
                        Server.SayToAllChat($"{red}{onPlayerKill.Killer.Name} {white}não aguentou mais e explodiu. =C");
                        break;
                    }
                    else if (kills > 2)
                    {
                        Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}se explodiu com {red}{kills} {white}pessoas.");
                    }
                    c4SuicideKillerIds.RemoveAll(suicider => suicider == onPlayerKill.Killer.SteamID);
                    break;
                }
                c4SuicideKillerIds.Add(onPlayerKill.Killer.SteamID);
                break;
            case "C4":
            case "Anti Personnel Mine":
            case "Frag Grenade":
            case "Anti Vehicle Grenade":
            case "Claymore":
                if (!explosiveKills.Contains(onPlayerKill.Killer.SteamID))
                {
                    await Task.Delay(50);
                    var kills = explosiveKills.FindAll(suicider => suicider == onPlayerKill.Killer.SteamID).Count + 1;
                    if (kills == 3)
                    {
                        Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}explodiu {red}{kills} {white}pessoas com uma {magenta}{onPlayerKill.KillerTool}");
                    }
                    explosiveKills.RemoveAll(suicider => suicider == onPlayerKill.Killer.SteamID);
                    break;
                }
                explosiveKills.Add(onPlayerKill.Killer.SteamID);
                break;
            case "Flashbang":
                Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}jogou uma {magenta}flashbang{white} no coco de {red}{onPlayerKill.Victim.Name}");
                break;

        }
        
        if (!killstreak.ContainsKey(onPlayerKill.Killer.SteamID)) killstreak.Add(onPlayerKill.Killer.SteamID, 0);
        if (onPlayerKill.Killer != onPlayerKill.Victim) killstreak[onPlayerKill.Killer.SteamID]++;

        switch (killstreak[onPlayerKill.Killer.SteamID])
        {
            case 10:
                Server.SayToAllChat($"{blue}{onPlayerKill.Killer.Name} {white}esta dominando com {blue}10 {white}abates.");
                break;
            case 14:
                Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}esta invencível com {green}14 {white}abates.");
                break;
            case 18:
                Server.SayToAllChat($"{magenta}{onPlayerKill.Killer.Name} {white}esta lendário com {magenta}18 {white}abates.");
                break;
            case 22:
                Server.SayToAllChat($"{gold}{onPlayerKill.Killer.Name} {white}esta divino com {gold}22 {white}abates.");
                break;
            case 50:
                Server.SayToAllChat($"{red}{onPlayerKill.Killer.Name} {white}esta provavelmente com hack {red}50{white} abates.");
                break;
        }

        float distance = Vector3.Distance(onPlayerKill.VictimPosition, onPlayerKill.KillerPosition);
        if (distance >= 1500f)
        {
            Server.SayToAllChat($"{green}{onPlayerKill.Killer.Name} {white}matou {red}{onPlayerKill.Victim.Name} {white}com uma distancia de: {blue}{distance:0.00}{white} metros.");
        }

        await Task.CompletedTask;
    }

    public override Task OnPlayerDied(RunnerPlayer player)
    {
        if (!killstreak.ContainsKey(player.SteamID)) killstreak.Add(player.SteamID, 0);
        killstreak[player.SteamID] = 0;
        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        if (!killstreak.ContainsKey(player.SteamID)) killstreak.Add(player.SteamID, 0);
        return Task.CompletedTask;
    }
    public override Task OnPlayerDisconnected(RunnerPlayer player)
    {
        killstreak.Remove(player.SteamID);
        return Task.CompletedTask;
    }
}