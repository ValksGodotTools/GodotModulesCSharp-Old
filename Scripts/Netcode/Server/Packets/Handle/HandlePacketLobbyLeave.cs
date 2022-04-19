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

            if (!Players.ContainsKey(peer.ID))
            {
                GDLog($"Received LobbyLeave packet from peer with id {peer.ID}. Tried to remove id {peer.ID} from Players but does not exist to begin with");
                return;
            }

            Players.Remove(peer.ID);

            // tell other players that this player left lobby
            Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyLeave, new WPacketLobbyLeave {
                Id = peer.ID
            }, GetOtherPeers(peer.ID)));
        }
    }
}