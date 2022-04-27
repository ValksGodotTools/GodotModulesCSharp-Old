using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyGameStart : APacketServer
    {
        public override void Handle()
        {
            if (!SceneManager.InLobby())
                return;

            if (GameClient.IsHost) 
            {
                GameServer.EmitClientPositions.Enabled = true;
            }

            SceneManager.ChangeScene("Game");
        }
    }
}