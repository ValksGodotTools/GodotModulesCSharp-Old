using ENet;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode
{
    public class CPacketLobby : APacketClient
    {
        public LobbyOpcode LobbyOpcode { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)LobbyOpcode);

            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    writer.Write(Message);
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    writer.Write(CountdownRunning);
                    break;

                case LobbyOpcode.LobbyJoin:
                    writer.Write(Username);
                    break;

                case LobbyOpcode.LobbyReady:
                    writer.Write(Ready);
                    break;
            }
        }

        public override void Read(PacketReader reader)
        {
            LobbyOpcode = (LobbyOpcode)reader.ReadByte();

            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    Message = reader.ReadString();
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    CountdownRunning = reader.ReadBool();
                    break;

                case LobbyOpcode.LobbyJoin:
                    Username = reader.ReadString();
                    break;

                case LobbyOpcode.LobbyReady:
                    Ready = reader.ReadBool();
                    break;
            }
        }

        public override void Handle(Peer peer)
        {
            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    HandleChatMessage(peer);
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    HandleCountdownChange(peer);
                    break;

                case LobbyOpcode.LobbyGameStart:
                    HandleGameStart(peer);
                    break;

                case LobbyOpcode.LobbyJoin:
                    HandleJoin(peer);
                    break;

                case LobbyOpcode.LobbyReady:
                    HandleReady(peer);
                    break;
            }
        }

        // LobbyChatMessage
        public string Message { get; set; }

        private void HandleChatMessage(Peer peer)
        {
            NetworkManager.GameServer.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyChatMessage,
                Id = peer.ID,
                Message = Message
            });
        }

        // LobbyCountdownChange
        public bool CountdownRunning { get; set; }

        private void HandleCountdownChange(Peer peer)
        {
            NetworkManager.GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyCountdownChange,
                CountdownRunning = CountdownRunning
            });
        }

        // LobbyGameStart
        private void HandleGameStart(Peer peer)
        {
            NetworkManager.GameServer.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby { LobbyOpcode = LobbyOpcode.LobbyGameStart });
        }

        // LobbyJoin
        public string Username { get; set; }

        private void HandleJoin(Peer peer)
        {
            // Check if data.Username is appropriate username
            // TODO

            var isHost = false;

            if (NetworkManager.GameServer.Players.Count == 0)
                isHost = true;

            // Keep track of joining player server side
            if (NetworkManager.GameServer.Players.ContainsKey(peer.ID))
            {
                NetworkManager.GameServer.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                return;
            }

            NetworkManager.GameServer.Players.Add(peer.ID, new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = isHost
            });

            // tell joining player their Id and tell them about other players in lobby
            // also tell them if they are the host or not
            NetworkManager.GameServer.Send(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyInfo,
                Id = peer.ID,
                IsHost = isHost,
                Players = NetworkManager.GameServer.GetOtherPlayers(peer.ID)
            }, peer);

            // tell other players about new player that joined
            NetworkManager.GameServer.SendToOtherPeers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyJoin,
                Id = peer.ID,
                Username = Username
            });
        }

        // LobbyReady
        public bool Ready { get; set; }

        private void HandleReady(Peer peer)
        {
            var player = NetworkManager.GameServer.Players[peer.ID];
            player.Ready = Ready;

            NetworkManager.GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyReady,
                Id = peer.ID,
                Ready = Ready
            });
        }
    }
}