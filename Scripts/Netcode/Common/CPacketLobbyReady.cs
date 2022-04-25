using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyReady : IPacketClient 
    {
        public bool Ready { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Ready);
        }

        public void Read(PacketReader reader)
        {
            Ready = reader.ReadBool();
        }

        public void Handle(ENet.Peer peer)
        {
            var player = GameServer.Players[peer.ID];
            player.Ready = Ready;

            GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.LobbyReady, new SPacketLobbyReady {
                Id = peer.ID,
                Ready = Ready
            });
        }
    }
}