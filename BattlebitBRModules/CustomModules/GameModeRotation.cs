using BBRAPIModules;
using Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.7
/// </summary>

public class GameModeRotation : BattleBitModule
{
    public GameModeRotationConfiguration Configuration { get; set; }

    public override Task OnConnected()
    {
        this.Server.GamemodeRotation.SetRotation(this.Configuration.GameModes);

        return Task.CompletedTask;
    }

    //Comando de informação de rotação de modos

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    [CommandCallback("GameModes", Description = "Shows the current gamemode rotation")]
    public void GameModes(RunnerPlayer commandSource)
    {
        string modes = "";
        foreach (var mode in Configuration.GameModes)
        {
            modes += mode + ", ";
        }
        Server.MessageToPlayer(commandSource, $"A rotação de modos atual é: {modes}");
    }
}

public class GameModeRotationConfiguration : ModuleConfiguration
{
    public string[] GameModes { get; set; } = new[]
    {
        "TDM",
        "AAS",
        "RUSH",
        "CONQ",
        "DOMI",
        "ELI",
        "INFCONQ",
        "FRONTLINE",
        "GunGameFFA",
        "FFA",
        "GunGameTeam",
        "SuicideRush",
        "CatchGame",
        "Infected",
        "CashRun",
        "VoxelFortify",
        "VoxelTrench",
        "CaptureTheFlag"
    };
}