using GodotModules.Netcode;

namespace GodotModules.Netcode.Client 
{
    public class RPacketLobbyJoin
    {
        public uint Id { get; set; }
        public string Username { get; set; }

        public RPacketLobbyJoin(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            Username = reader.ReadString();
        }
    }
}