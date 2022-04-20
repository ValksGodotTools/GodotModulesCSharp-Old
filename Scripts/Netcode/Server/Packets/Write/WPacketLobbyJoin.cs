using GodotModules.Netcode;

namespace GodotModules.Netcode.Server
{
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