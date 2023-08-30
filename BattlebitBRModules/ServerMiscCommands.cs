using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

namespace BattlebitBRModules
{
    public class Configuration : ModuleConfiguration
    {
        //items
        public bool AllowNV { get; set; } = true;
        public bool AllowFlashbang { get; set; } = true;
        //mechanics
        public bool AllowKillFeed { get; set; } = true;
        public bool Bleeding { get; set; } = true;
        public bool Stamina { get; set; } = false;
        //weapons
    }

    [RequireModule(typeof(CommandHandler))]
    public class ServerMiscCommands : BattleBitModule
    {
        public Configuration ServerMiscConfig { get; set; }
        [ModuleReference]
        public CommandHandler commandHandler { get; set; }

        public override void OnModulesLoaded()
        {
            commandHandler.Register(this);
        }

        [CommandCallback("TogItem", Description = "Toggles an server item on", AllowedRoles = Roles.Admin)]
        public void TogItem(RunnerPlayer commandSource, string item, bool on)
        {
            switch (item)
            {
                case "nv":
                case "nightvision":
                    Server.AnnounceShort("Nightvision is now " + (on ? "enabled" : "disabled"));
                    ServerMiscConfig.AllowNV = on;
                    foreach (var player in Server.AllPlayers)
                    {
                        player.Modifications.CanUseNightVision = on;
                    }
                    break;
                case "flashbang":
                    ServerMiscConfig.AllowFlashbang = on;
                    Server.AnnounceShort("Flashbang is now " + (on ? "enabled" : "disabled"));
                    foreach (var player in Server.AllPlayers)
                    {
                        if (player.CurrentLoadout.Throwable.Name == "FlashBang")
                        {
                            player.SetLightGadget("Grenade", 0);
                        }
                    }
                    break;
            }
        }

        [CommandCallback("TogMechanic", Description = "Toggles an server mechanic on or off", AllowedRoles = Roles.Admin)]
        public void TogMechanic(RunnerPlayer commandSource, string mechanic, bool on)
        {
            switch (mechanic)
            {
                case "bleeding":
                    Server.AnnounceShort("Bleeding is now " + (on ? "enabled" : "disabled"));
                    ServerMiscConfig.Bleeding = on;
                    if (on)
                    {
                        foreach (var player in Server.AllPlayers)
                        {
                            player.Modifications.DisableBleeding();
                        }
                        break;
                    }                    
                    foreach (var player in Server.AllPlayers)
                    {
                        player.Modifications.DisableBleeding();
                    }     
                    break;
                case "killfeed":
                    Server.AnnounceShort("killfeed is now " + (on ? "allowed" : "blocked"));
                    ServerMiscConfig.AllowKillFeed = on;
                    break;
                case "stamina":
                    Server.AnnounceShort("stamina is now " + (on ? "enabled" : "disabled"));
                    ServerMiscConfig.Stamina = on;
                    foreach (var player in Server.AllPlayers)
                    {
                        player.Modifications.StaminaEnabled = on;
                    }
                    break;
            }
        }

        [CommandCallback("KillFeed", Description = "Toggles an server mechanic on or off", AllowedRoles = Roles.Admin)]
        public void KillFeed(RunnerPlayer commandSource, bool on)
        {
            if (!ServerMiscConfig.AllowKillFeed)
            {
                Server.MessageToPlayer(commandSource.SteamID, "KillFeed is disabled on this server");
                return;
            }
            commandSource.Modifications.KillFeed = on;
        }

        public override Task OnPlayerSpawned(RunnerPlayer player)
        {
            if (player.CurrentLoadout.Throwable.Name == "FlashBang")
            {
                player.SetLightGadget("Grenade", 0);
            }
            return Task.CompletedTask;
        }

        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            player.Modifications.CanUseNightVision = ServerMiscConfig.AllowNV;
            player.Modifications.KillFeed = ServerMiscConfig.AllowKillFeed && player.Modifications.KillFeed;
            if (ServerMiscConfig.Bleeding) player.Modifications.EnableBleeding();
            else player.Modifications.DisableBleeding();
            player.Modifications.StaminaEnabled = ServerMiscConfig.Stamina;
            return Task.CompletedTask;
        }
    }
}