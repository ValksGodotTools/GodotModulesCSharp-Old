using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyGameStart : IPacketServer
    {
        public void Write(PacketWriter writer)
        {

        }

        public void Read(PacketReader reader)
        {

        }

        public void Handle()
        {
            Godot.GD.Print("GAME START");
        }
    }
}