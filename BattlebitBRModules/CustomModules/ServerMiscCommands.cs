using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System.Threading.Tasks;

/// <summary>
/// DO NOT USE, STILL IN DEVELOPMENT
/// </summary>
namespace BattlebitBRModules
{
    public class Configuration : ModuleConfiguration
    {
        //items
        public bool AllowNV { get; set; } = true;
        public bool AllowFlashbang { get; set; } = true;
        //mechanics
        public bool Bleeding { get; set; } = true;
        public bool StaminaEnabled { get; set; } = false;

        public float BleedMinLife { get; set; } = 40;
        public float HpPerBandage { get; set; } = 40;
        public float ReloadSpeedMultiplier { get; set; } = 1f;
        public float RunningSpeedMultiplier { get; set; } = 1f;
        public float ReceiveDamageMultiplier { get; set; } = 1f;
        public float CaptureFlagSpeedMultiplier { get; set; } = 1f;
        public float DownTimeGiveUpTime { get; set; } = 60f;
        public float RespawnTime { get; set; } = 10;

        public float JumpHeightMultiplier { get; set; } = 1;
        public float FallDamageMultiplier { get; set; } = 1;
        public bool AirStrafe { get; set; } = true;
    }

    [RequireModule(typeof(CommandHandler)), RequireModule(typeof(HudCommands))]
    public class ServerMiscCommands : BattleBitModule
    {
        public Configuration ServerMiscConfig { get; set; }
        [ModuleReference]
        public CommandHandler commandHandler { get; set; }
        [ModuleReference]
        public HudCommands hudCommands { get; set; }
        public override void OnModulesLoaded()
        {
            commandHandler.Register(this);
        }

        [CommandCallback("Item", Description = "Toggles an server item on", AllowedRoles = Roles.Admin)]
        public void Item(RunnerPlayer commandSource, string item, bool on)
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
                        if (player.CurrentLoadout.Throwable == Gadgets.Flashbang)
                        {
                            player.SetLightGadget(Gadgets.FragGrenade.Name, 0);
                        }
                    }
                    break;
            }
        }

        [CommandCallback("Mechanic", Description = "Toggles an server mechanic on or off", AllowedRoles = Roles.Admin)]
        public void Mechanic(RunnerPlayer commandSource, string mechanic, bool on)
        {
            switch (mechanic)
            {
                case "bleeding":
                case "bleed":
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
                case "stamina":
                    Server.AnnounceShort("stamina is now " + (on ? "enabled" : "disabled"));
                    ServerMiscConfig.StaminaEnabled = on;
                    foreach (var player in Server.AllPlayers)
                    {
                        player.Modifications.StaminaEnabled = on;
                    }
                    break;
            }
        }

        [CommandCallback("Halo", Description = "oooooOOOO°°°°OOOO°°°°OOOoooooo", AllowedRoles = Roles.Admin)]
        public void Halo(RunnerPlayer commandSource, bool on)
        {
            if (on)
            {
                foreach (var player in Server.AllPlayers)
                {
                    ServerMiscConfig.AirStrafe = player.Modifications.AirStrafe = true;
                    ServerMiscConfig.JumpHeightMultiplier = player.Modifications.JumpHeightMultiplier = 3;
                    ServerMiscConfig.FallDamageMultiplier = player.Modifications.FallDamageMultiplier = 0;
                }
                return;
            }
            foreach (var player in Server.AllPlayers)
            {
                ServerMiscConfig.JumpHeightMultiplier = player.Modifications.JumpHeightMultiplier = 1;
                ServerMiscConfig.FallDamageMultiplier = player.Modifications.FallDamageMultiplier = 1;
            }
        }

        [CommandCallback("GameDifficulty",
            Description = "Changes some mechanics to make the game more casual or more hardcore. args = (c)asual|(n)ormal|(h)ardcore",
            AllowedRoles = Roles.Admin)]
        public void GameDifficulty(RunnerPlayer commandSource, string difficulty)
        {
            switch (difficulty)
            {
                case "hardcore":
                case "h":
                    ServerMiscConfig.AirStrafe = false;
                    hudCommands.HudConfig.PointLogHudEnabled = false;
                    hudCommands.HudConfig.FriendlyHUDEnabled = false;
                    hudCommands.HudConfig.HitMarkersEnabled = false;
                    ServerMiscConfig.StaminaEnabled = true;
                    ServerMiscConfig.BleedMinLife = 75;
                    ServerMiscConfig.Bleeding = true;
                    ServerMiscConfig.HpPerBandage = 20;
                    ServerMiscConfig.ReloadSpeedMultiplier = 0.8f;
                    ServerMiscConfig.RunningSpeedMultiplier = 0.9f;
                    ServerMiscConfig.ReceiveDamageMultiplier = 1.1f;
                    ServerMiscConfig.FallDamageMultiplier = 1.2f;
                    ServerMiscConfig.CaptureFlagSpeedMultiplier = 0.9f;
                    ServerMiscConfig.DownTimeGiveUpTime = 15f;
                    ServerMiscConfig.RespawnTime = 10;
                    break;
                case "normal":
                case "n":
                    ServerMiscConfig.AirStrafe = true;
                    hudCommands.HudConfig.PointLogHudEnabled = true;
                    hudCommands.HudConfig.FriendlyHUDEnabled = true;
                    hudCommands.HudConfig.HitMarkersEnabled = true;
                    ServerMiscConfig.StaminaEnabled = false;
                    ServerMiscConfig.BleedMinLife = 40;
                    ServerMiscConfig.Bleeding = true;
                    ServerMiscConfig.HpPerBandage = 40f;
                    ServerMiscConfig.ReloadSpeedMultiplier = 1f;
                    ServerMiscConfig.RunningSpeedMultiplier = 1f;
                    ServerMiscConfig.ReceiveDamageMultiplier = 1f;
                    ServerMiscConfig.FallDamageMultiplier = 1f;
                    ServerMiscConfig.CaptureFlagSpeedMultiplier = 1f;
                    ServerMiscConfig.DownTimeGiveUpTime = 60f;
                    ServerMiscConfig.RespawnTime = 10;
                    break;
                case "casual":
                case "c":
                    ServerMiscConfig.AirStrafe = true;
                    hudCommands.HudConfig.PointLogHudEnabled = true;
                    hudCommands.HudConfig.FriendlyHUDEnabled = true;
                    hudCommands.HudConfig.HitMarkersEnabled = true;
                    ServerMiscConfig.StaminaEnabled = false;
                    ServerMiscConfig.Bleeding = false;
                    ServerMiscConfig.HpPerBandage = 50f;
                    ServerMiscConfig.ReloadSpeedMultiplier = 1.1f;
                    ServerMiscConfig.RunningSpeedMultiplier = 1.1f;
                    ServerMiscConfig.ReceiveDamageMultiplier = 0.9f;
                    ServerMiscConfig.FallDamageMultiplier = 0.9f;
                    ServerMiscConfig.CaptureFlagSpeedMultiplier = 1.2f;
                    ServerMiscConfig.DownTimeGiveUpTime = 90f;
                    ServerMiscConfig.RespawnTime = 4;                    
                    break;
            }
            foreach (var player in Server.AllPlayers)
            {
                player.Modifications.AirStrafe = ServerMiscConfig.AirStrafe;
                player.Modifications.PointLogHudEnabled = true;
                player.Modifications.FriendlyHUDEnabled = hudCommands.HudConfig.FriendlyHUDEnabled;
                player.Modifications.HitMarkersEnabled = hudCommands.HudConfig.HitMarkersEnabled;
                player.Modifications.StaminaEnabled = ServerMiscConfig.StaminaEnabled;
                player.Modifications.HpPerBandage = ServerMiscConfig.HpPerBandage;
                player.Modifications.ReloadSpeedMultiplier = ServerMiscConfig.ReloadSpeedMultiplier;
                player.Modifications.RunningSpeedMultiplier = ServerMiscConfig.RunningSpeedMultiplier;
                player.Modifications.ReceiveDamageMultiplier = ServerMiscConfig.ReceiveDamageMultiplier;
                player.Modifications.FallDamageMultiplier = ServerMiscConfig.FallDamageMultiplier;
                player.Modifications.CaptureFlagSpeedMultiplier = ServerMiscConfig.CaptureFlagSpeedMultiplier;
                player.Modifications.DownTimeGiveUpTime = ServerMiscConfig.DownTimeGiveUpTime;
                player.Modifications.RespawnTime = ServerMiscConfig.CaptureFlagSpeedMultiplier;
                if (ServerMiscConfig.Bleeding)
                {
                    player.Modifications.EnableBleeding(ServerMiscConfig.BleedMinLife);
                }
                else
                {
                    player.Modifications.DisableBleeding();
                }
            }
        }

        public override Task OnPlayerSpawned(RunnerPlayer player)
        {
            if (!ServerMiscConfig.AllowFlashbang && player.CurrentLoadout.Throwable == Gadgets.Flashbang)
            {
                player.SetLightGadget(Gadgets.FragGrenade.Name, 0);
            }
            return Task.CompletedTask;
        }

        public override Task OnPlayerConnected(RunnerPlayer player)
        {
            player.Modifications.CanUseNightVision = ServerMiscConfig.AllowNV;
            player.Modifications.JumpHeightMultiplier = ServerMiscConfig.JumpHeightMultiplier;

            player.Modifications.AirStrafe = ServerMiscConfig.AirStrafe;
            player.Modifications.PointLogHudEnabled = true;
            player.Modifications.FriendlyHUDEnabled = hudCommands.HudConfig.FriendlyHUDEnabled;
            player.Modifications.HitMarkersEnabled = hudCommands.HudConfig.HitMarkersEnabled;
            player.Modifications.StaminaEnabled = ServerMiscConfig.StaminaEnabled;
            player.Modifications.HpPerBandage = ServerMiscConfig.HpPerBandage;
            player.Modifications.ReloadSpeedMultiplier = ServerMiscConfig.ReloadSpeedMultiplier;
            player.Modifications.RunningSpeedMultiplier = ServerMiscConfig.RunningSpeedMultiplier;
            player.Modifications.ReceiveDamageMultiplier = ServerMiscConfig.ReceiveDamageMultiplier;
            player.Modifications.FallDamageMultiplier = ServerMiscConfig.FallDamageMultiplier;
            player.Modifications.CaptureFlagSpeedMultiplier = ServerMiscConfig.CaptureFlagSpeedMultiplier;
            player.Modifications.DownTimeGiveUpTime = ServerMiscConfig.DownTimeGiveUpTime;
            player.Modifications.RespawnTime = ServerMiscConfig.CaptureFlagSpeedMultiplier;
            if (ServerMiscConfig.Bleeding)
            {
                player.Modifications.EnableBleeding(ServerMiscConfig.BleedMinLife);
            }
            else
            {
                player.Modifications.DisableBleeding();
            }
            return Task.CompletedTask;
        }
    }
}