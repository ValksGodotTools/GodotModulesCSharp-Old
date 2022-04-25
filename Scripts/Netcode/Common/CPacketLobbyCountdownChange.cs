using GodotModules.Netcode.Server;
using System.Linq;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyCountdownChange : IPacketClient 
    {
        public bool CountdownRunning { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(CountdownRunning);
        }

        public void Read(PacketReader reader)
        {
            CountdownRunning = reader.ReadBool();
        }

        public void Handle(ENet.Peer peer)
        {
            GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.LobbyCountdownChange, new SPacketLobbyCountdownChange {
                Id = peer.ID,
                CountdownRunning = CountdownRunning
            });
        }
    }
}