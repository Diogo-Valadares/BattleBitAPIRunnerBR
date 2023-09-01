using BBRAPIModules;
using System;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami
/// Version: 0.4.7
/// </summary>

public class Announcements : BattleBitModule
{
    public AnnouncementsTexts AnnouncementsConfig { get; set; }

    private int lastIndex = 0;
    private DateTime lastAnnouncement = DateTime.Today;

    public override Task OnTick()
    {
        if (lastAnnouncement.AddSeconds(AnnouncementsConfig.SecondsBetweenAnouncements) > DateTime.Now)
        {
            Server.SayToAllChat(AnnouncementsConfig.Announcements[lastIndex]);
            lastIndex = (lastIndex + 1 == AnnouncementsConfig.Announcements.Length) ? 0 : lastIndex + 1;
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