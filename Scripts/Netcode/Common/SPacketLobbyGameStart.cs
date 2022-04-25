using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyGameStart : APacketServer
    {
        public override void Handle()
        {
            SceneManager.ChangeScene("Game");
        }
    }
}