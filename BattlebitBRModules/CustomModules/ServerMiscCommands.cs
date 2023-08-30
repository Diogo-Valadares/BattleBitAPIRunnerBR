﻿using BattleBitAPI.Common;
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
        public bool KillFeed { get; set; } = true;
        public bool Bleeding { get; set; } = true;
        public bool StaminaEnabled { get; set; } = false;

        public bool FriendlyHUDEnabled { get; set; } = true;
        public bool HitMarkersEnabled { get; set; } = true;
        public bool PointLogHudEnabled { get; set; } = true;
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
                        if (player.CurrentLoadout.Throwable == Gadgets.Flashbang)
                        {
                            player.SetLightGadget(Gadgets.FragGrenade.Name, 0);
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
                    ServerMiscConfig.KillFeed = on;
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

        [CommandCallback("KillFeed", Description = "Allows any player to turn killfeed on or off")]
        public void KillFeed(RunnerPlayer commandSource, bool on)
        {
            if (!ServerMiscConfig.KillFeed)
            {
                Server.MessageToPlayer(commandSource.SteamID, "KillFeed is disabled on this server");
                return;
            }
            commandSource.Modifications.KillFeed = on;
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
                    ServerMiscConfig.KillFeed = false;
                    ServerMiscConfig.AirStrafe = false;
                    ServerMiscConfig.PointLogHudEnabled = false;
                    ServerMiscConfig.FriendlyHUDEnabled = false;
                    ServerMiscConfig.HitMarkersEnabled = false;
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
                    ServerMiscConfig.KillFeed = true;
                    ServerMiscConfig.AirStrafe = true;
                    ServerMiscConfig.PointLogHudEnabled = true;
                    ServerMiscConfig.FriendlyHUDEnabled = true;
                    ServerMiscConfig.HitMarkersEnabled = true;
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
                    ServerMiscConfig.KillFeed = true;
                    ServerMiscConfig.AirStrafe = true;
                    ServerMiscConfig.PointLogHudEnabled = true;
                    ServerMiscConfig.FriendlyHUDEnabled = true;
                    ServerMiscConfig.HitMarkersEnabled = true;
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
                player.Modifications.KillFeed = ServerMiscConfig.KillFeed;
                player.Modifications.AirStrafe = ServerMiscConfig.AirStrafe;
                player.Modifications.PointLogHudEnabled = true;
                player.Modifications.FriendlyHUDEnabled = ServerMiscConfig.FriendlyHUDEnabled;
                player.Modifications.HitMarkersEnabled = ServerMiscConfig.HitMarkersEnabled;
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

            player.Modifications.KillFeed = ServerMiscConfig.KillFeed;
            player.Modifications.AirStrafe = ServerMiscConfig.AirStrafe;
            player.Modifications.PointLogHudEnabled = true;
            player.Modifications.FriendlyHUDEnabled = ServerMiscConfig.FriendlyHUDEnabled;
            player.Modifications.HitMarkersEnabled = ServerMiscConfig.HitMarkersEnabled;
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