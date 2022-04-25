using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyReady : APacketClient 
    {
        public bool Ready { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Ready);
        }

        public override void Read(PacketReader reader)
        {
            Ready = reader.ReadBool();
        }

        public override void Handle(ENet.Peer peer)
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