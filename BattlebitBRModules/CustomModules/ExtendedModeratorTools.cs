using BattleBitAPI.Common;
using BBRAPIModules;
using Commands;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami expanded by @_dx2
/// </summary>

[RequireModule(typeof(CommandHandler))]
[RequireModule(typeof(PlayerStatsCache))]
[RequireModule(typeof(DatabaseComunicator))]
[Module("Extension of the moderator tools module, adding a database communication", "1.0")]
public class ExtendedModeratorTools : BattleBitModule
{
    [ModuleReference]
    public CommandHandler CommandHandler { get; set; }
    [ModuleReference]
    public DatabaseComunicator DatabaseComunicator { get; set; }
    [ModuleReference]
    public PlayerStatsCache PlayerStatsCache { get; set; }

    public override void OnModulesLoaded()
    {
        this.CommandHandler.Register(this);
    }

    public override Task OnConnected()
    {
        Task.Run(playerInspection);

        return Task.CompletedTask;
    }

    private async void playerInspection()
    {
        while (this.IsLoaded && this.Server.IsConnected)
        {
            foreach (KeyValuePair<RunnerPlayer, RunnerPlayer> inspection in this.inspectPlayers)
            {
                RunnerPlayer target = inspection.Value;

                StringBuilder playerInfo = new();
                playerInfo.AppendLine($"{target.Name} ({target.SteamID} - {target.Role}");
                playerInfo.AppendLine($"Net: {target.PingMs}ms");
                playerInfo.AppendLine($"Game: {target.Team} - {target.SquadName} - {(target.IsConnected ? "Connected" : "Disconnected")}");
                playerInfo.AppendLine($"Health: {target.HP} - {(target.IsAlive ? "Alive" : "Dead")} - {(target.IsDown ? "Down" : "Up")} - {(target.IsBleeding ? "Bleeding" : "Not bleeding")}");
                playerInfo.AppendLine($"State: {target.StandingState} - {target.LeaningState} - {(target.InVehicle ? "In vehicle" : "Not in vehicle")}");
                playerInfo.AppendLine($"Position: {target.Position}");
                playerInfo.AppendLine($"Loadout: {target.CurrentLoadout.PrimaryWeapon.ToolName} - {target.CurrentLoadout.SecondaryWeapon.ToolName} - {target.CurrentLoadout.ThrowableName}");
                playerInfo.AppendLine($"Loadout: {target.CurrentLoadout.HeavyGadgetName} - {target.CurrentLoadout.LightGadgetName}");

                inspection.Key.Message(playerInfo.ToString());
            }

            await Task.Delay(250);
        }
    }

    [CommandCallback("Say", Description = "Prints a message to all players", AllowedRoles = Roles.Moderator)]
    public void Say(RunnerPlayer commandSource, string message)
    {
        this.Server.SayToAllChat(message);
    }

    [CommandCallback("AnnounceShort", Description = "Prints a short announce to all players", AllowedRoles = Roles.Moderator)]
    public void AnnounceShort(RunnerPlayer commandSource, string message)
    {
        this.Server.AnnounceShort(message);
    }

    [CommandCallback("AnnounceLong", Description = "Prints a long announce to all players", AllowedRoles = Roles.Moderator)]
    public void AnnounceLong(RunnerPlayer commandSource, string message)
    {
        this.Server.AnnounceLong(message);
    }

    [CommandCallback("Warn", Description = "Messages a specific player", AllowedRoles = Roles.Moderator)]
    public void Warn(RunnerPlayer commandSource, RunnerPlayer target, string warn)
    {
        target.WarnPlayer(warn);
        commandSource.Message($"Warn sent to {target.Name}", 10);
    }

    [CommandCallback("Message", Description = "Messages a specific player", AllowedRoles = Roles.Moderator)]
    public void Message(RunnerPlayer commandSource, RunnerPlayer target, string message, float? timeout = null)
    {
        if (timeout.HasValue)
        {
            target.Message(message, timeout.Value);
        }
        else
        {
            target.Message(message);
        }

        commandSource.Message($"Message sent to {target.Name}", 10);
    }

    [CommandCallback("Clear", Description = "Clears the chat", AllowedRoles = Roles.Moderator)]
    public void Clear(RunnerPlayer commandSource)
    {
        this.Server.SayToAllChat("".PadLeft(30, '\n') + "<size=0%>Chat cleared");
    }

    [CommandCallback("Kick", Description = "Kicks a player", AllowedRoles = Roles.Moderator)]
    public void Kick(RunnerPlayer commandSource, RunnerPlayer target, string reason)
    {
        target.Kick(reason);
        commandSource.Message($"Player {target.Name} kicked", 10);
    }

    [CommandCallback("Ban", Description = "Bans a player", AllowedRoles = Roles.Moderator)]
    public async void Ban(RunnerPlayer commandSource, RunnerPlayer target, string reason, int? minutes = null)
    {
        if (!PlayerStatsCache.Cache.TryGetValue(target.SteamID, out var playerStats))
        {
            commandSource.Message($"Jogador não encontrado no cache, tente novamente", 10);
            return;
        }
        if (playerStats.BannedUntil != null && minutes == null)
        {
            commandSource.Message($"Player {target.Name} is already banned");
            return;
        }
        playerStats.BannedUntil = minutes != null ? DateTime.Now.AddMinutes(minutes.Value) : DateTime.MaxValue;
        await DatabaseComunicator.RestrictPlayer(DatabaseComunicator.BanType.ban, target.SteamID, reason, commandSource.SteamID, minutes ?? int.MaxValue);
        commandSource.Message($"Player {target.Name} banned for{(minutes != null ? $" {minutes} minutes." : "ever.")}", 10);
        target.Kick($"Você foi banido {(minutes != null ? $"por {minutes} minutos" : "para sempre")} com o motivo:{reason}");
    }

    [CommandCallback("Gag", Description = "Gags a player", AllowedRoles = Roles.Moderator)]
    public async void Gag(RunnerPlayer commandSource, RunnerPlayer target, string reason, int? minutes = null)
    {
        if (!PlayerStatsCache.Cache.TryGetValue(target.SteamID, out var playerStats))
        {
            commandSource.Message($"Jogador não encontrado no cache, tente novamente", 10);
            return;
        }
        if (playerStats.GaggedUntil != null && minutes == null)
        {
            commandSource.Message($"Player {target.Name} is already gagged");
            return;
        }
        playerStats.GaggedUntil = minutes != null ? DateTime.Now.AddMinutes(minutes.Value) : DateTime.MaxValue;
        await DatabaseComunicator.RestrictPlayer(DatabaseComunicator.BanType.gag, target.SteamID, reason, commandSource.SteamID, minutes ?? int.MaxValue);
        commandSource.Message($"Player {target.Name} gagged for {(minutes != null ? $" {minutes} minutes." : "ever.")}", 10);
        target.Message($"Você foi impedido de usar o chat {(minutes != null ? $"por {minutes} minutos." : "para sempre.")}\n Motivo = {reason}", 10);
    }

    [CommandCallback("Mute", Description = "Mutes a player", AllowedRoles = Roles.Moderator)]
    public async void Mute(RunnerPlayer commandSource, RunnerPlayer target, string reason, int? minutes = null)
    {
        if (!PlayerStatsCache.Cache.TryGetValue(target.SteamID, out var playerStats))
        {
            commandSource.Message($"Jogador não encontrado no cache, tente novamente", 10);
            return;
        }
        if (playerStats.MutedUntil != null && minutes == null)
        {
            commandSource.Message($"Player {target.Name} is already muted");
            return;
        }
        playerStats.MutedUntil = minutes != null ? DateTime.Now.AddMinutes(minutes.Value) : DateTime.MaxValue;
        await DatabaseComunicator.RestrictPlayer(DatabaseComunicator.BanType.mute, target.SteamID, reason, commandSource.SteamID, minutes ?? int.MaxValue);
        commandSource.Message($"Player {target.Name} muted for {(minutes != null ? $" {minutes} minutes." : "ever.")}", 10);
        target.Message($"Você foi mutado {(minutes != null ? $"por {minutes} minutos." : "para sempre.")}\n Motivo = {reason}", 10);
        target.Modifications.IsVoiceChatMuted = true;
    }

    [CommandCallback("Silence", Description = "Mutes and gags a player", AllowedRoles = Roles.Moderator)]
    public void Silence(RunnerPlayer commandSource, RunnerPlayer target, string reason, int? minutes = null)
    {
        Mute(commandSource, target, reason, minutes);
        Gag(commandSource, target, reason, minutes);
    }

    [CommandCallback("Kill", Description = "Kills a player", AllowedRoles = Roles.Moderator)]
    public void Kill(RunnerPlayer commandSource, RunnerPlayer target)
    {
        target.Kill();

        commandSource.Message($"Player {target.Name} killed", 10);
    }

    [CommandCallback("Unban", Description = "Unbans a player", AllowedRoles = Roles.Moderator)]
    public async void Unban(RunnerPlayer commandSource, ulong target)
    {
        if (await DatabaseComunicator.GetRestriction(DatabaseComunicator.BanType.ban, target) != null)
        {
            await DatabaseComunicator.UnrestrictPlayer(DatabaseComunicator.BanType.ban, target);
            commandSource.Message($"Player {target} unbanned", 10);
        }
        else
        {
            commandSource.Message($"Player {target} is not banned or does not exist", 10);
        }
    }

    [CommandCallback("Ungag", Description = "Ungags a player", AllowedRoles = Roles.Moderator)]
    public async void Ungag(RunnerPlayer commandSource, RunnerPlayer target)
    {
        if (!PlayerStatsCache.Cache.TryGetValue(target.SteamID, out var playerStats))
        {
            commandSource.Message($"Jogador não encontrado no cache, tente novamente", 10);
            return;
        }
        if (playerStats.GaggedUntil == null)
        {
            commandSource.Message($"Player {target.Name} is not gagged");
            return;
        }
        await DatabaseComunicator.UnrestrictPlayer(DatabaseComunicator.BanType.gag, target.SteamID);
        playerStats.GaggedUntil = null;
        commandSource.Message($"Player {target.Name} ungagged", 10);
        target.Message("Você agora pode usar o chat", 10);

    }

    [CommandCallback("Unmute", Description = "Unmutes a player", AllowedRoles = Roles.Moderator)]
    public async void Unmute(RunnerPlayer commandSource, RunnerPlayer target)
    {
        if (!target.Modifications.IsVoiceChatMuted)
        {
            commandSource.Message($"Player {target.Name} is not muted");
            return;
        }
        if (!PlayerStatsCache.Cache.TryGetValue(target.SteamID, out var playerStats))
        {
            commandSource.Message($"Jogador não encontrado no cache, tente novamente", 10);
            return;
        }
        target.Modifications.IsVoiceChatMuted = false;
        await DatabaseComunicator.UnrestrictPlayer(DatabaseComunicator.BanType.mute, target.SteamID);
        playerStats.MutedUntil = null;
        commandSource.Message($"Player {target.Name} unmuted", 10);
        target.Message("Você foi desmutado", 10);
    }

    [CommandCallback("Unsilence", Description = "Unmutes and ungags a player", AllowedRoles = Roles.Moderator)]
    public void Unsilence(RunnerPlayer commandSource, RunnerPlayer target)
    {
        Unmute(commandSource, target);
        Ungag(commandSource, target);
    }

    [CommandCallback("LockSpawn", Description = "Prevents a player or all players from spawning", AllowedRoles = Roles.Moderator)]
    public void LockSpawn(RunnerPlayer commandSource, RunnerPlayer? target = null)
    {
        if (target == null)
        {
            this.globalSpawnLock = true;
            foreach (RunnerPlayer player in this.Server.AllPlayers)
            {
                player.Modifications.CanDeploy = false;
            }
            commandSource.Message("Spawn globally locked", 10);
        }
        else
        {
            if (this.lockedSpawns.Contains(target.SteamID))
            {
                commandSource.Message($"Spawn already locked for {target.Name}", 10);
                return;
            }

            target.Modifications.CanDeploy = false;
            this.lockedSpawns.Add(target.SteamID);
            commandSource.Message($"Spawn locked for {target.Name}", 10);
        }
    }

    [CommandCallback("UnlockSpawn", Description = "Allows a player or all players to spawn", AllowedRoles = Roles.Moderator)]
    public void UnlockSpawn(RunnerPlayer commandSource, RunnerPlayer? target = null)
    {
        if (target == null)
        {
            this.globalSpawnLock = false;
            foreach (RunnerPlayer player in this.Server.AllPlayers)
            {
                player.Modifications.CanDeploy = true;
            }
            commandSource.Message("Spawn globally unlocked", 10);
        }
        else
        {
            if (!this.lockedSpawns.Contains(target.SteamID))
            {
                commandSource.Message($"Spawn already unlocked for {target.Name}", 10);
                return;
            }
            this.lockedSpawns.Remove(target.SteamID);
            commandSource.Message($"Spawn unlocked for {target.Name}", 10);
        }
    }

    [CommandCallback("tp2me", Description = "Teleports a player to you", AllowedRoles = Roles.Moderator)]
    public void TeleportPlayerToMe(RunnerPlayer commandSource, RunnerPlayer target)
    {
        target.Teleport(new Vector3((int)commandSource.Position.X, (int)commandSource.Position.Y, (int)commandSource.Position.Z));
    }

    [CommandCallback("tpme2", Description = "Teleports you to a player", AllowedRoles = Roles.Moderator)]
    public void TeleportMeToPlayer(RunnerPlayer commandSource, RunnerPlayer target)
    {
        commandSource.Teleport(new Vector3((int)target.Position.X, (int)target.Position.Y, (int)target.Position.Z));
    }

    [CommandCallback("tp", Description = "Teleports a player to another player", AllowedRoles = Roles.Moderator)]
    public void TeleportPlayerToPlayer(RunnerPlayer commandSource, RunnerPlayer target, RunnerPlayer destination)
    {
        target.Teleport(new Vector3((int)destination.Position.X, (int)destination.Position.Y, (int)destination.Position.Z));
    }

    [CommandCallback("tp2pos", Description = "Teleports a player to a position", AllowedRoles = Roles.Moderator)]
    public void TeleportPlayerToPos(RunnerPlayer commandSource, RunnerPlayer target, int x, int y, int z)
    {
        target.Teleport(new Vector3(x, y, z));
    }

    [CommandCallback("tpme2pos", Description = "Teleports you to a position", AllowedRoles = Roles.Moderator)]
    public void TeleportMeToPos(RunnerPlayer commandSource, int x, int y, int z)
    {
        commandSource.Teleport(new Vector3(x, y, z));
    }

    [CommandCallback("freeze", Description = "Freezes a player", AllowedRoles = Roles.Moderator)]
    public void Freeze(RunnerPlayer commandSource, RunnerPlayer target, string? message = null)
    {
        target.Modifications.Freeze = true;
        commandSource.Message($"Player {target.Name} frozen", 10);

        if (!string.IsNullOrEmpty(message))
        {
            target.Message(message);
        }
    }

    [CommandCallback("unfreeze", Description = "Unfreezes a player", AllowedRoles = Roles.Moderator)]
    public void Unfreeze(RunnerPlayer commandSource, RunnerPlayer target, string? message = null)
    {
        target.Modifications.Freeze = false;
        commandSource.Message($"Player {target.Name} unfrozen", 10);

        if (!string.IsNullOrEmpty(message))
        {
            target.Message(message);
        }
    }

    private Dictionary<RunnerPlayer, RunnerPlayer> inspectPlayers = new();

    [CommandCallback("Inspect", Description = "Inspects a player or stops inspection", AllowedRoles = Roles.Moderator)]
    public void Inspect(RunnerPlayer commandSource, RunnerPlayer? target = null)
    {
        if (target is null)
        {
            this.inspectPlayers.Remove(commandSource);
            commandSource.Message("Inspection stopped", 2);
            return;
        }

        if (this.inspectPlayers.ContainsKey(commandSource))
        {
            this.inspectPlayers[commandSource] = target;
        }
        else
        {
            this.inspectPlayers.Add(commandSource, target);
        }
    }

    private List<ulong> lockedSpawns = new();
    private bool globalSpawnLock = false;


    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string msg)
    {
        if (!PlayerStatsCache.Cache.ContainsKey(player.SteamID)) return Task.FromResult(true);
        if (PlayerStatsCache.Cache[player.SteamID].GaggedUntil > DateTime.Now)
        {
            player.Message($"Você esta bloqueado de mandar mensagem no chat até {PlayerStatsCache.Cache[player.SteamID].GaggedUntil}", 10);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
    public override async Task OnPlayerConnected(RunnerPlayer player)
    {
        var banReason = await DatabaseComunicator.GetRestriction(DatabaseComunicator.BanType.ban, player.SteamID);
        if (banReason != null)
        {
            Server.Kick(player.SteamID, $"Você foi banido por \"{banReason[0]}\", até {banReason[1]}");
        }
    }
    public override Task OnRoundStarted()
    {
        foreach (var player in Server.AllPlayers)
        {
            if (!PlayerStatsCache.Cache.ContainsKey(player.SteamID)) continue;
            player.Modifications.IsVoiceChatMuted = PlayerStatsCache.Cache[player.SteamID].MutedUntil != null && PlayerStatsCache.Cache[player.SteamID].MutedUntil > DateTime.Now;
        }
        return Task.CompletedTask;
    }
    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(RunnerPlayer player, OnPlayerSpawnArguments request)
    {
        if (this.globalSpawnLock || this.lockedSpawns.Contains(player.SteamID))
        {
            return Task.FromResult<OnPlayerSpawnArguments?>(null);
        }

        return Task.FromResult(request as OnPlayerSpawnArguments?);
    }
}
