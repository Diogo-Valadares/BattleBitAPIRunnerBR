using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BattlebitBRModules
{
    public class Configuration : ModuleConfiguration
    {
        public bool AllowNV { get; set; } = true;
        public bool KillFeed { get; set; } = true;
        public bool Bleeding { get; set; } = true;
        public bool Stamina { get; set; } = false;
    }

    [RequireModule(typeof(CommandHandler))]
    public class ServerMiscCommands : BattleBitModule
    {
        public Configuration ServerMiscConfig { get; set; }

        [CommandCallback("TogItemOn", Description = "Toggles an server item on", AllowedRoles = Roles.Admin)]
        public void TogItemOn(RunnerPlayer commandSource, string item)
        {
            switch (item)
            {
                case "nv":
                case "nightvision":
                    ServerMiscConfig.AllowNV = true;
                    break;
            }
        }
        [CommandCallback("TogItemOff", Description = "Toggles an server item on", AllowedRoles = Roles.Admin)]
        public void TogItemOff(RunnerPlayer commandSource, string item)
        {
            switch (item)
            {
                case "nv":
                case "nightvision":
                    ServerMiscConfig.AllowNV = false;
                    break;
            }
        }

        [CommandCallback("TogMechanic on", Description = "Toggles an server mechanic on or off", AllowedRoles = Roles.Admin)]
        public void TogMechanicOn(RunnerPlayer commandSource, string mechanic)
        {
            switch (mechanic)
            {
                case "bleeding":
                    ServerMiscConfig.Bleeding = true;
                    break;
                case "killfeed":
                    ServerMiscConfig.KillFeed = true;
                    break;
                case "stamina":
                    ServerMiscConfig.Stamina = true;
                    break;
            }
        }
        [CommandCallback("TogMechanic off", Description = "Toggles an server mechanic on or off", AllowedRoles = Roles.Admin)]
        public void TogMechanicOff(RunnerPlayer commandSource, string mechanic)
        {
            switch (mechanic)
            {
                case "bleeding":
                    ServerMiscConfig.Bleeding = false;
                    break;
                case "killfeed":
                    ServerMiscConfig.KillFeed = false;
                    break;
                case "stamina":
                    ServerMiscConfig.Stamina = false;
                    break;
            }
        }

        public override async Task OnPlayerSpawned(RunnerPlayer player)
        {
            player.Modifications.CanUseNightVision = ServerMiscConfig.AllowNV;
            player.Modifications.KillFeed = ServerMiscConfig.KillFeed;
            if (ServerMiscConfig.Bleeding) player.Modifications.EnableBleeding();
            else player.Modifications.DisableBleeding();
            player.Modifications.StaminaEnabled = ServerMiscConfig.Stamina;
            await Task.CompletedTask;
        }
    }
}