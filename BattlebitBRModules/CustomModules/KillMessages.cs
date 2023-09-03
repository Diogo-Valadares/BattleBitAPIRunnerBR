using BattleBitAPI.Common;
using BBRAPIModules;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// Version: 1.0
/// </summary>
/// 
namespace BattlebitBRModules;

public class KillMessages : BattleBitModule
{
    List<ulong> c4killerIds = new();
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<RunnerPlayer> onPlayerKill)
    {
        switch (onPlayerKill.KillerTool)
        {
            case "SledgeHammer":
            case "SledgeHammerSkinA":
            case "SledgeHammerSkinB":
            case "SledgeHammerSkinC":
            case "Pickaxe":
            case "PickaxeIronPickaxe":
                Server.SayToAllChat(onPlayerKill.Killer.Name + " passou a lambida em " + onPlayerKill.Victim.Name);
                break;
            case "SuicideC4":
                if(onPlayerKill.Killer == onPlayerKill.Victim)
                {
                    await Task.Delay(200);
                    var kills = c4killerIds.FindAll(suicider => suicider == onPlayerKill.Killer.SteamID);
                    if(kills.Count == 0)
                    {
                        Server.SayToAllChat(onPlayerKill.Killer.Name + " não aguentou mais e explodiu. =C");
                    }
                    Server.SayToAllChat(onPlayerKill.Killer.Name + " se explodiu com " + (kills.Count) + " pessoas");
                    c4killerIds.RemoveAll(suicider => suicider == onPlayerKill.Killer.SteamID);
                    break;
                }
                c4killerIds.Add(onPlayerKill.Killer.SteamID);        
                break;
        }
        float distance = Vector3.Distance(onPlayerKill.VictimPosition, onPlayerKill.KillerPosition);
        if (distance >= 1500f)
        {
            Server.SayToAllChat(onPlayerKill.Killer.Name + " matou " + onPlayerKill.Victim.Name + " com uma distancia de: " + distance);
        }

        await Task.CompletedTask;
    }
}