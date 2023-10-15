using BBRAPIModules;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// DO NOT USE, STILL IN DEVELOPMENT
/// </summary>
namespace BattleBitBaseModules;

[Module(@"Adds a bridge between the server and a MySQL database. 
It's not supposed to be used by iteself, only as an extension of other modules", "1.0")]
public class DatabaseComunicator : BattleBitModule
{
    public enum BanType { ban, mute, gag }
    public enum Preference { Killfeed }
    public readonly List<string> PlayerOptions = new() { "Killfeed" };

    public DatabaseConfig Config { get; set; }

    public async Task<MySqlConnection> GetConnection()
    {        
        MySqlConnection connection = new(Config.connectionString);
        bool connected = false;
        int tries = 0;
        while (!connected && tries < 5)
        {
            try
            {
                await connection.OpenAsync();
                connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Server?.ServerName} DatabaseCommunicator: failed to set a communication with the database, {ex.Message}. Retrying connection({tries}).");
                await Task.Delay(20);
            }
        }        
        return connection;
    }

    public override async void OnModulesLoaded()
    {
        using MySqlConnection connection = await GetConnection();
        Console.WriteLine($"DatabaseCommunicator: successfully connected to database");
        var show = $"show tables";
        var command = new MySqlCommand(show, connection);
        var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        Console.Write($"DatabaseCommunicator: The available tables are {reader.GetString(0)}");
        while (await reader.ReadAsync())
        {
            var tableName = reader.GetString(0);
            Console.Write(", " + tableName);
        }
        Console.WriteLine();
        await reader.CloseAsync();
        connection.Close();
    }

    /*
    [DEPRECATED]public async Task<bool> AddPlayer(ulong player, string playerName)
    {
        var insert = "insert into player (steamId, Name, Kills, Deaths, Revives, TimesRevived, Headshots, Points, Killfeed) " +
            $"values ({player}, @playername, 0, 0, 0, 0, 0, 0, 1);";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(insert, connection);
        command.Parameters.Add("@playername", MySqlDbType.VarString);
        command.Parameters["@playername"].Value = playerName;
        var result = await command.ExecuteNonQueryAsync();
        command.Dispose();
        connection.Close();
        return result == 1;
    }
    [DEPRECATED]public async Task<bool> UpdatePlayer(ulong playerID, Dictionary<string, object> fields)
    {
        var update = $" UPDATE player SET Name = @PlayerName, Kills = {fields["Kills"]}, Deaths = {fields["Deaths"]}, Revives = {fields["Revives"]}, " +
            $"Headshots = {fields["Headshots"]}, TimesRevived = {fields["TimesRevived"]}, Points = {fields["Points"]},Killfeed = {fields["Killfeed"]} WHERE steamID = {playerID};";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(update, connection);
        command.Parameters.Add("@PlayerName", MySqlDbType.VarString);
        command.Parameters["@PlayerName"].Value = ((RunnerPlayer)fields["player"]).Name;
        var result = await command.ExecuteNonQueryAsync();
        command.Dispose();
        connection.Close();
        return result == 1;
    }
    */

    public async Task<Dictionary<string, object>> GetPlayer(ulong player)
    {
        var returnFields = new Dictionary<string, object>();

        var select = $"select * from player p where p.steamID = {player};";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(select, connection);
        var result = await command.ExecuteReaderAsync();
        while (result.Read())
        {
            for (int i = 0; i < result.FieldCount; i++)
            {
                returnFields.Add(result.GetName(i), result.GetValue(i));
            }
        }
        await result.CloseAsync();
        select = $"select r.Type, r.ReleaseDate from restriction r where Player_steamID = {player} and ReleaseDate > now();";
        command = new MySqlCommand(select, connection);
        result = await command.ExecuteReaderAsync();
        var restrictions = new Dictionary<string, object>();
        var lastkey = "";
        while (result.Read())
        {
            for (int i = 0; i < result.FieldCount; i++)
            {
                if (result.GetName(i).Equals("Type"))
                {
                    lastkey = (string)result.GetValue(i);
                }
                else
                {
                    restrictions.Add(lastkey, result.GetValue(i));
                }
            }
        }

        if (restrictions.Count > 0)
        {
            returnFields.Add("Restrictions", restrictions);
        }

        await result.CloseAsync();
        connection.Close();
        return returnFields;
    }
    public async Task<bool> UpdatePlayers(List<Dictionary<string, object>> fields)
    {
        bool result;
        MySqlConnection connection = new(Config.connectionString);
        try
        {
            await connection.OpenAsync();

            var update = new StringBuilder("insert into player(steamId, Name, Kills, Deaths, Revives, TimesRevived, Headshots, Points, Killfeed) values");
            var command = new MySqlCommand();
            foreach (var player in fields)
            {
                var playerSteamID = ((RunnerPlayer)player["player"]).SteamID;
                update.Append($"({playerSteamID}, @PlayerName{playerSteamID}, {player["Kills"]}, {player["Deaths"]}, {player["Revives"]}, " +
                $"{player["TimesRevived"]}, {player["Headshots"]}, {player["Points"]}, {player["Killfeed"]}),");
                command.Parameters.Add($"@PlayerName{playerSteamID}", MySqlDbType.VarString);
                command.Parameters[$"@PlayerName{playerSteamID}"].Value = ((RunnerPlayer)player["player"]).Name;
            }
            update.Remove(update.Length - 1, 1);
            update.Append("as new\r\n" +
                "ON DUPLICATE KEY UPDATE\r\n" +
                "player.Name = new.Name, player.Kills = player.Kills + new.Kills, \r\n" +
                "player.Deaths = player.Deaths + new.Deaths, player.Revives = player.Revives + new.Revives, \r\n" +
                "player.TimesRevived = player.TimesRevived + new.timesRevived, player.Headshots = player.Headshots + new.Headshots, \r\n" +
                "player.Points = player.Points + new.Points; ");

            command.Connection = connection;
            command.CommandText = update.ToString();
            result = await command.ExecuteNonQueryAsync() > 0;
            command.Dispose();
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while trying to update SQL Database: " + ex.ToString());
            return false;
        }
        return result;
    }

    #region Restriction Related Methods
    public async Task<bool> RestrictPlayer(BanType bantype, ulong bannedUser, string reason, ulong bannedByAdmin, int time)
    {
        var insert = "insert into Restriction(Type, Reason, Date, ReleaseDate, Player_steamID, Admin_steamID) " +
            $"values (\"{bantype}\",@Reason,@date,@ReleaseDate,{bannedUser},{bannedByAdmin});";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(insert, connection);
        command.Parameters.Add("@Reason", MySqlDbType.VarString);
        command.Parameters["@Reason"].Value = reason;
        command.Parameters.Add("@Date", MySqlDbType.DateTime);
        command.Parameters["@Date"].Value = DateTime.Now;
        command.Parameters.Add("@ReleaseDate", MySqlDbType.DateTime);
        command.Parameters["@ReleaseDate"].Value = DateTime.Now.AddMinutes(time);
        var result = await command.ExecuteNonQueryAsync();
        command.Dispose();
        connection.Close();
        return result == 1;
    }
    public async Task<bool> UnrestrictPlayer(BanType bantype, ulong bannedUser)
    {
        var update = $"UPDATE Restriction SET ReleaseDate = now() WHERE Player_steamID = {bannedUser} and Type = \"{bantype}\";";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(update, connection);
        var result = await command.ExecuteNonQueryAsync();
        command.Dispose();
        connection.Close();
        return result == 1;
    }
    public async Task<object[]> GetRestriction(BanType bantype, ulong player)
    {
        var select = $"select r.Reason, r.ReleaseDate from restriction r where Player_steamID = {player} and ReleaseDate > now() and Type = \"{bantype}\";";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(select, connection);
        var result = await command.ExecuteReaderAsync();
        if (!result.Read()) return null;
        object[] output = new object[2];
        do
        {
            for (int i = 0; i < result.FieldCount; i++)
            {
                if (result.GetName(i).Equals("Reason"))
                {
                    output[0] = (string)result.GetValue(i);
                }
                if (result.GetName(i).Equals("ReleaseDate"))
                {
                    output[1] = (DateTime)result.GetValue(i);
                }
            }
        }
        while (result.Read());
        await result.CloseAsync();
        connection.Close();
        return output;
    }
    #endregion
    #region Preferences Related Methods
    public async Task<bool> UpdatePreference(string preference, ulong player, int value)
    {
        var update = $"update player set {preference} = {value} where player.steamId = {player};";
        using MySqlConnection connection = await GetConnection();
        var command = new MySqlCommand(update, connection);
        var result = await command.ExecuteNonQueryAsync();
        command.Dispose();
        connection.Close();
        return result == 1;
    }
    #endregion
}
public class DatabaseConfig : ModuleConfiguration
{
    public string connectionString { get; set; } =
        "Server=127.0.0.1,3306;Database=mydb;User Id=gameServer;Password=$#@Vsdvst4waAWERoa;";
}
