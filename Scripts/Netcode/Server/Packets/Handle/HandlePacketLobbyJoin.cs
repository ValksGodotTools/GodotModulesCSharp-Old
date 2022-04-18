using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    public class HandlePacketLobbyJoin : HandlePacket
    {
        public override void Handle(Peer peer, PacketReader reader)
        {
            var data = new RPacketLobbyJoin(reader);

            // Keep track of joining player server side
            Players.Add(peer.ID, data.Username);

            // Get all players EXCEPT for peer.ID
            var otherPlayers = GetOtherPlayers(peer.ID);

            GDLog($"Other Players {Utils.StringifyDict(otherPlayers)}");

            // Tell joining player about all the other players in the lobby
            if (otherPlayers.Count > 0)
                Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyList, new WPacketLobbyList
                {
                    Players = otherPlayers
                }, peer));

            // Tell everyone including player this player joined
            Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyJoin, new WPacketLobbyJoin
            {
                Id = peer.ID,
                Username = data.Username
            }, Peers.Values.ToArray()));
        }
    }
}