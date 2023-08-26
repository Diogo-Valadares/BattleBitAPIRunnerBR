using BattleBitAPI.Common;
using BBRAPIModules;
using System.Linq;
using System.Threading.Tasks;

namespace BattlebitBRModules
{
    public class AdminModule : BattleBitModule
    {
        private readonly ulong[] moderatorsID = {
            76561197987717578,//goim                                                  
            76561198105203764//dx2
            //adicionar mais aqui depois
        };

        private bool allowNV = true;
        private bool killFeed = true;

        public override async Task OnPlayerSpawned(RunnerPlayer player)
        {
            player.Modifications.CanUseNightVision = allowNV;
            player.Modifications.KillFeed = killFeed;
            await Task.CompletedTask;
        }

        public override async Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string msg)
        {
            if (!msg.StartsWith('!')) return await Task.FromResult(false);
            var command = msg.Split(" ");

            switch (command[0])
            {
                case "!item":
                    if (moderatorsID.ToList().Contains(player.SteamID))
                        ItemCommand(player, command);
                    else
                        Server.MessageToPlayer(player.SteamID, "Você não tem permissão para usar esse comando");
                    break;
                case "!option":
                    if (moderatorsID.ToList().Contains(player.SteamID))
                        OptionCommand(player, command);
                    else
                        Server.MessageToPlayer(player.SteamID, "Você não tem permissão para usar esse comando");
                    break;       
                case "!ping":
                    Server.MessageToPlayer(player, "Pong!");
                    break;
            }
            return await Task.FromResult(true);
        }

        private void ItemCommand(RunnerPlayer player, string[] command)
        {
            switch (command[1])
            {
                case "NV":
                case "NightVision":
                    allowNV = command[1] != "ban";
                    break;
            }
        }

        private void OptionCommand(RunnerPlayer player, string[] command)
        {
            switch (command[1])
            {
                case "KillFeed":
                    killFeed = command[1] != "off";
                    break;
            }
        }
    }
}