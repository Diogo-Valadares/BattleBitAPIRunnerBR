using BBRAPIModules;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// DO NOT USE, STILL IN DEVELOPMENT
/// </summary>
namespace BattleBitBaseModules;

public class PlayerPreferences : BattleBitModule
{
    public PlayerPreferencesConfiguration Configuration { get; set; }

    public bool AddKeys(string[] keys)
    {
        foreach (var key in keys)
        {
            if (Configuration.Keys.Contains(key))
            {
                Console.WriteLine($"PlayerPreferences: Key {key} already exists, skipping");
            }
        }

        Configuration.Keys = Configuration.Keys.Concat(keys).ToArray();
        Configuration.Save();
        return true;
    }
    public bool RemoveKeys(string[] keys)
    {
        List<int> indicesToRemove = new();
        foreach (var key in keys)
        {
            int index = Array.IndexOf(keys, key);
            if (index < 0)
            {
                Console.WriteLine($"PlayerPreferences: Key {key} does not exists, skipping");
            }
            else
            {
                indicesToRemove.Add(index);
            }
        }
        


        Configuration.Save();
        return true;
    }

    public void ChangeValue(ulong playerID, (string key, string value)[] info)
    {
        /*if (!Configuration.Values.(p => ulong.Parse(p[0]) == playerID))

            foreach (var player in Configuration.Values)

                foreach (var key in info)
                {
                    if (Configuration.)
        }*/
    }
    public void RemoveValue()
    {
    }
}

public class PlayerPreferencesConfiguration : ModuleConfiguration
{
    public string[] Keys { get; set; } =
    {
        "PlayerID"
    };

    public string[][] Values { get; set; } = Array.Empty<string[]>();
}
