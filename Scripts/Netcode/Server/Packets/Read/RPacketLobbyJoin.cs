using Common.Netcode;

namespace GodotModules.Netcode.Server
{
    public class RPacketLobbyJoin
    {
        public string Username { get; set; }

        public RPacketLobbyJoin(PacketReader reader)
        {
            Username = reader.ReadString();
        }
    }
}