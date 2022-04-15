using Common.Netcode;

namespace GodotModules.Netcode.Client
{
    public class WPacketLobbyJoin : IWritable
    {
        public string Username { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Username);
        }
    }
}