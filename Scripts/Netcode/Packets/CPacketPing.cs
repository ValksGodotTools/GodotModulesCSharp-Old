using LiteNetLib;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode
{
    public class CPacketPing : APacketClient
    {
        public override void Handle(GameServer server, NetPeer peer)
        {
            server.Send(ServerPacketOpcode.Pong, peer);
        }
    }
}