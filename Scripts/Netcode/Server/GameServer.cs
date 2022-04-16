using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    // All game specific logic will be put in here
    public class GameServer : ENetServer
    {
        public Dictionary<uint, string> Players { get; set; } // the players in a lobby

        public override void _Ready()
        {
            base._Ready();
            Players = new Dictionary<uint, string>();
        }

        protected override void Connect(Event netEvent)
        {
        }

        protected override void Receive(Event netEvent, ClientPacketOpcode opcode, PacketReader reader)
        {
            GDLog($"Received new client packet: {opcode}");
            
            if (opcode == ClientPacketOpcode.LobbyJoin)
            {
                var data = new RPacketLobbyJoin(reader);

                // add player to lobby
                Players.Add(netEvent.Peer.ID, data.Username);

                // update lobby player list UI
                GodotCmds.Enqueue(new GodotCmd
                {
                    Opcode = GodotOpcode.AddPlayerToLobbyList,
                    Data = data.Username
                });

                // tell joining player about all the other players in the lobby
                var otherPlayers = new Dictionary<uint, string>(Players);
                otherPlayers.Remove(netEvent.Peer.ID);

                if (otherPlayers.Count > 0)
                    Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyList, new WPacketLobbyList {
                        Players = otherPlayers
                    }, netEvent.Peer));

                // tell other players (including player to tell player about their ID) that this player joined
                Outgoing.Enqueue(new ServerPacket((byte)ServerPacketOpcode.LobbyJoin, new WPacketLobbyJoin {
                    Id = netEvent.Peer.ID,
                    Username = data.Username
                }, Peers.Values.ToArray()));
            }
        }

        protected override void Disconnect(Event netEvent)
        {
            //GameServer.Players.Remove(netEvent.Peer.ID);
        }

        protected override void Timeout(Event netEvent)
        {
            //GameServer.Players.Remove(netEvent.Peer.ID);
        }

        protected override void Stopped()
        {
        }
    }
}