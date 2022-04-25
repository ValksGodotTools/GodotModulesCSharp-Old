using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyGameStart : APacketClient 
    {
        public override void Handle(ENet.Peer peer)
        {
            GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyGameStart);
        }
    }
}