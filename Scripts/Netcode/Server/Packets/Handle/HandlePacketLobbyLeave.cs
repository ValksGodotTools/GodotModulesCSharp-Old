using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    public class HandlePacketLobbyLeave : HandlePacket
    {
        public override void Handle(Peer peer, PacketReader reader)
        {
            GDLog($"Received lobby leave packet from {peer.ID}");

            Players.Remove(peer.ID);

            // tell other players that this player left lobby
            Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyLeave, new WPacketLobbyLeave {
                Id = peer.ID
            }, GetOtherPeers(peer.ID)));
        }
    }
}