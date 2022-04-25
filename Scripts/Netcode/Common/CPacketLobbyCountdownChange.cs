using GodotModules.Netcode.Server;
using System.Linq;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyCountdownChange : APacketClient 
    {
        public bool CountdownRunning { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(CountdownRunning);
        }

        public override void Read(PacketReader reader)
        {
            CountdownRunning = reader.ReadBool();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.LobbyCountdownChange, new SPacketLobbyCountdownChange {
                Id = peer.ID,
                CountdownRunning = CountdownRunning
            });
        }
    }
}