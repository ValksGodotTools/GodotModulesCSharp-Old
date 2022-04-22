using GodotModules.Netcode.Server;

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

            // Keep track of joining player server side
            if (GameServer.Players.ContainsKey(peer.ID)) 
            {
                GameServer.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                return;
            }

            GameServer.Players.Add(peer.ID, Username);

            // tell joining player their Id and tell them about other players in lobby
            GameServer.Send(ServerPacketOpcode.LobbyInfo, new SPacketLobbyInfo {
                Id = peer.ID,
                Players = GameServer.GetOtherPlayers(peer.ID)
            }, peer);

            // tell other players about new player that joined
            GameServer.Send(ServerPacketOpcode.LobbyJoin, new SPacketLobbyJoin {
                Id = peer.ID,
                Username = Username
            }, GameServer.GetOtherPeers(peer.ID));
        }
    }
}