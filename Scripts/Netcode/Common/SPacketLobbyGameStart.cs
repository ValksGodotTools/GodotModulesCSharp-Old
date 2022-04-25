using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyGameStart : APacketServer
    {
        public override void Handle()
        {
            Godot.GD.Print("GAME START");
        }
    }
}