using BattleBitAPI;
using BattleBitAPI.Common;
using BBRAPIModules;
using System.Numerics;
using System.Threading.Tasks;

namespace Classes;

public class KillMessages : BattleBitModule
{
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<RunnerPlayer> onPlayerKill)
    {
        if(onPlayerKill.KillerTool == Gadgets.SledgeHammer || onPlayerKill.KillerTool == Gadgets.Pickaxe)
        {
            Server.SayToAllChat(onPlayerKill.Killer.Name + " passou a lambida em " + onPlayerKill.Victim.Name);
            await Task.CompletedTask;
            return;
        }        
        
        float distance = Vector3.Distance(onPlayerKill.VictimPosition, onPlayerKill.KillerPosition);

        if (distance >= 100f)
        {
            Server.SayToAllChat(onPlayerKill.Killer.Name + " matou " + onPlayerKill.Victim.Name + " com uma distancia de: " + distance);
            await Task.CompletedTask;
        }
        else
        {
            Server.SayToAllChat(onPlayerKill.Killer.Name + " matou em uma distancia inferior a 100 metros");
            await Task.CompletedTask;
        }
    }
}