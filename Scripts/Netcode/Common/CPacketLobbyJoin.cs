using GodotModules.Netcode.Server;
using System.Linq;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyJoin : IPacketClient
    {
        public string Username { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Username);
        }

        public void Read(PacketReader reader)
        {
            Username = reader.ReadString();
        }

        public void Handle(ENet.Peer peer)
        {
            // Check if data.Username is appropriate username
            // TODO

            var isHost = false;

            if (GameServer.Players.Count == 0)
                isHost = true;

            // Keep track of joining player server side
            if (GameServer.Players.ContainsKey(peer.ID)) 
            {
                GameServer.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                return;
            }

            GameServer.Players.Add(peer.ID, new DataPlayer {
                Username = Username,
                Ready = false,
                Host = isHost
            });

            // tell joining player their Id and tell them about other players in lobby
            // also tell them if they are the host or not
            GameServer.Send(ServerPacketOpcode.LobbyInfo, new SPacketLobbyInfo {
                Id = peer.ID,
                IsHost = isHost,
                Players = GameServer.GetOtherPlayers(peer.ID)
            }, peer);

            // tell other players about new player that joined
            GameServer.SendToOtherPeers(peer.ID, ServerPacketOpcode.LobbyJoin, new SPacketLobbyJoin {
                Id = peer.ID,
                Username = Username
            });
        }
    }
}