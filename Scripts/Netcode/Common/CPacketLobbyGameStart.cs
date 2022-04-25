using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyGameStart : IPacketClient 
    {
        public void Write(PacketWriter writer)
        {
            
        }

        public void Read(PacketReader reader)
        {
            
        }

        public void Handle(ENet.Peer peer)
        {
            GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyGameStart);
        }
    }
}