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

            // add player to lobby
            GameManager.GameServer.Players.Add(peer.ID, data.Username);

            // update lobby player list UI
            GameServer.GodotCmds.Enqueue(new GodotCmd
            {
                Opcode = GodotOpcode.AddPlayerToLobbyList,
                Data = data.Username
            });

            // tell joining player about all the other players in the lobby
            var otherPlayers = new Dictionary<uint, string>(GameManager.GameServer.Players);
            otherPlayers.Remove(peer.ID);

            if (otherPlayers.Count > 0)
                GameServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyList, new WPacketLobbyList
                {
                    Players = otherPlayers
                }, peer));

            // tell other players (including player to tell player about their ID) that this player joined
            GameServer.Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyJoin, new WPacketLobbyJoin
            {
                Id = peer.ID,
                Username = data.Username
            }, GameServer.Peers.Values.ToArray()));
        }
    }
}