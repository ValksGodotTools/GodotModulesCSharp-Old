using Common.Netcode;

namespace Valk.Modules.Netcode.Server
{
    public class RPacketLobbyJoin : IReadable
    {
        public string Username { get; set; }

        public void Read(PacketReader reader)
        {
            Username = reader.ReadString();
        }
    }
}
