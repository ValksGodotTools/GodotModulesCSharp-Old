using Common.Netcode;

namespace Valk.Modules.Netcode.Client
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