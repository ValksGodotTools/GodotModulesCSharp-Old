using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyGameStart : APacketServer
    {
        public override void Handle()
        {
            if (GameClient.IsHost) 
            {
                GameServer.TimerGameLoop.Enabled = true;
                GameServer.TimerNotifyClients.Enabled = true;
            }

            SceneManager.ChangeScene("Game");
        }
    }
}