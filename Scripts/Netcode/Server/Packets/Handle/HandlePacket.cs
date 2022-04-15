using Common.Netcode;
using ENet;

namespace Valk.Modules.Netcode.Server
{
    public class HandlePacket
    {
        public static void LobbyJoin(Event netEvent, ClientPacketOpcode opcode, PacketReader reader)
        {
            var data = new RPacketLobbyJoin();
            data.Read(reader);

            GameServer.Players.Add(netEvent.Peer.ID, data.Username);
            UILobby.AddPlayer(data.Username);
        }
    }
}
