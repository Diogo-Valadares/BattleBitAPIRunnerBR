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
    }

    [RequireModule(typeof(CommandHandler))]
    public class ServerMiscCommands : BattleBitModule
    {
        public Configuration ServerMiscConfig { get; set; }

        [CommandCallback("TogModifier", Description = "Toggles an server modifier on or off", AllowedRoles = Roles.Admin)]            
        public void TogModifier(RunnerPlayer commandSource, string modifier, bool on)
        {
            switch (modifier)
            {
                case "nv":
                case "nightvision":
                    ServerMiscConfig.AllowNV = on;
                    break;
            }
        }

        [CommandCallback("TogItem", Description = "Toggles an server modifier on or off", AllowedRoles = Roles.Admin)]
        public void TogItem(RunnerPlayer commandSource, string modifier, bool on)
        {
            switch (modifier)
            {
            }
        }

        [CommandCallback("TogMechanic", Description = "Toggles an server modifier on or off", AllowedRoles = Roles.Admin)]
        public void TogMechanic(RunnerPlayer commandSource, string modifier, bool on)
        {
            switch (modifier)
            {
                case "bleeding":
                    ServerMiscConfig.Bleeding = on;
                    break;
                case "killfeed":
                    ServerMiscConfig.KillFeed = on;
                    break;
            }
        }

        public override async Task OnPlayerSpawned(RunnerPlayer player)
        {
            player.Modifications.CanUseNightVision = ServerMiscConfig.AllowNV;
            player.Modifications.KillFeed = ServerMiscConfig.KillFeed;
            if (ServerMiscConfig.Bleeding) player.Modifications.EnableBleeding();
            else player.Modifications.DisableBleeding();
            await Task.CompletedTask;
        }
    }
}