using Common.Netcode;

namespace GodotModules.Netcode.Server
{
    // notify other players that this player joined the lobby
    public class WPacketLobbyJoin : IWritable 
    {
        public uint Id { get; set; }
        public string Username { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((string)Username);
        }
    }
}