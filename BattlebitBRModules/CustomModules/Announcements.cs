using BBRAPIModules;
using System;
using System.Threading.Tasks;

/// <summary>
/// Author: @_dx2
/// Version: 1.0
/// </summary>
/// 
namespace BattleBitBaseModules;

public class Announcements : BattleBitModule
{
    public AnnouncementsTexts AnnouncementsConfig { get; set; }

    private int lastIndex = 0;
    private DateTime lastAnnouncement;

    public override void OnModulesLoaded()
    {
        lastAnnouncement = DateTime.MinValue;
    }

    public override Task OnTick()
    {
        if (DateTime.Now > lastAnnouncement.AddSeconds(AnnouncementsConfig.SecondsBetweenAnouncements))
        {
            Server.SayToAllChat(AnnouncementsConfig.Announcements[lastIndex]);
            lastIndex = (lastIndex + 1 >= AnnouncementsConfig.Announcements.Length) ? 0 : lastIndex + 1;
            lastAnnouncement = DateTime.Now;
        }
        return Task.CompletedTask;
    }
}

public class AnnouncementsTexts : ModuleConfiguration
{
    public int SecondsBetweenAnouncements { get; set; } = 300;
    public string[] Announcements { get; set; } = new[]
    {
        "Este é um anúncio de testes, tenha um bom dia",
        "Se você deseja saber os comandos do servidor, utilize !help"
    };
}