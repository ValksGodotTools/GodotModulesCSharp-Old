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

                case LobbyOpcode.LobbyCreate:
                    writer.Write(Username);
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

                case LobbyOpcode.LobbyCreate:
                    Username = reader.ReadString();
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
                case LobbyOpcode.LobbyCreate:
                    HandleCreate(peer);
                    break;

                case LobbyOpcode.LobbyJoin:
                    HandleJoin(peer);
                    break;

                case LobbyOpcode.LobbyChatMessage:
                    HandleChatMessage(peer);
                    break;

                case LobbyOpcode.LobbyReady:
                    HandleReady(peer);
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    HandleCountdownChange(peer);
                    break;

                case LobbyOpcode.LobbyGameStart:
                    HandleGameStart(peer);
                    break;
            }
        }

        // LobbyCreate
        private void HandleCreate(Peer peer)
        {
            NetworkManager.GameServer.Players.Add((byte)peer.ID, new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = true
            });

            NetworkManager.GameServer.Send(ServerPacketOpcode.Lobby, new SPacketLobby {
                LobbyOpcode = LobbyOpcode.LobbyCreate
            }, peer);
        }

        // LobbyChatMessage
        public string Message { get; set; }

        private void HandleChatMessage(Peer peer)
        {
            NetworkManager.GameServer.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyChatMessage,
                Id = (byte)peer.ID,
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
            NetworkManager.GameServer.DisallowJoiningLobby = true;
            NetworkManager.GameServer.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby { LobbyOpcode = LobbyOpcode.LobbyGameStart });
        }

        // LobbyJoin
        public string Username { get; set; }

        private void HandleJoin(Peer peer)
        {
            // Check if data.Username is appropriate username
            // TODO

            // Keep track of joining player server side
            if (NetworkManager.GameServer.Players.ContainsKey((byte)peer.ID))
            {
                NetworkManager.GameServer.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                return;
            }

            if (NetworkManager.GameServer.DisallowJoiningLobby) 
            {
                NetworkManager.GameServer.Kick(peer.ID, DisconnectOpcode.Disconnected);
                NetworkManager.GameServer.Log($"Peer with id {peer.ID} tried to join lobby but game is running already");
                return;
            }

            NetworkManager.GameServer.Players.Add((byte)peer.ID, new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = false
            });

            // tell joining player their Id and tell them about other players in lobby
            // also tell them if they are the host or not
            NetworkManager.GameServer.Send(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyInfo,
                Id = (byte)peer.ID,
                Players = NetworkManager.GameServer.GetOtherPlayers((byte)peer.ID)
            }, peer);

            // tell other players about new player that joined
            NetworkManager.GameServer.SendToOtherPeers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyJoin,
                Id = (byte)peer.ID,
                Username = Username
            });
        }

        // LobbyReady
        public bool Ready { get; set; }

        private void HandleReady(Peer peer)
        {
            var player = NetworkManager.GameServer.Players[(byte)peer.ID];
            player.Ready = Ready;

            NetworkManager.GameServer.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyReady,
                Id = (byte)peer.ID,
                Ready = Ready
            });
        }
    }
}