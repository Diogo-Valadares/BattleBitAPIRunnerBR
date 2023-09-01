using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

public class HammersVSPickaxes : BattleBitModule
{
    public HvsPConfiguration Configuration { get; set; }

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("HvsP", Description = "Ativa e desativa o modo Martelos VS Picaretas", AllowedRoles = Roles.Admin)]
    public void HvsP(RunnerPlayer commandSource)
    {
        Configuration.IsModeOn = !Configuration.IsModeOn;
        Server.AnnounceShort($"Martelos VS Picaretas {(Configuration.IsModeOn ? "ativado" : "desligado")}");
        if (Configuration.IsModeOn)
        {
            foreach (var p in Server.AllPlayers)
            {
                p.SetNewRole(GameRole.Assault);
                p.SetThrowable("", 0, true);
                p.SetPrimaryWeapon(new WeaponItem(), 0, true);
                p.SetSecondaryWeapon(new WeaponItem(), 0, true);
                p.SetHeavyGadget("", 0, true);
                if (p.Team == Team.TeamA)
                {
                    p.SetLightGadget(Gadgets.Pickaxe.Name, 0);
                }
                else
                {
                    p.SetLightGadget(Gadgets.SledgeHammer.Name, 0);
                }
            }
        }
    }

    public override Task OnPlayerSpawned(RunnerPlayer player)
    {
        if (!Configuration.IsModeOn) return Task.CompletedTask;
        player.SetNewRole(GameRole.Assault);
        player.SetThrowable("", 0, true);
        player.SetPrimaryWeapon(new WeaponItem(), 0, true);
        player.SetSecondaryWeapon(new WeaponItem(), 0, true);
        player.SetHeavyGadget("", 0, true);
        if (player.Team == Team.TeamA)
        {
            player.SetLightGadget(Gadgets.Pickaxe.Name, 0);
        }
        else
        {
            player.SetLightGadget(Gadgets.SledgeHammer.Name, 0);
        }
        return Task.CompletedTask;
    }

    public class HvsPConfiguration : ModuleConfiguration
    {
        public bool IsModeOn { get; set; } = false;
    }

}
