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
                    writer.Write(LobbyName);
                    writer.Write(LobbyDescription);
                    break;

                case LobbyOpcode.LobbyJoin:
                    writer.Write(Username);
                    writer.Write(DirectConnect);
                    break;

                case LobbyOpcode.LobbyKick:
                    writer.Write(Id);
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
                    LobbyName = reader.ReadString();
                    LobbyDescription = reader.ReadString();
                    break;

                case LobbyOpcode.LobbyJoin:
                    Username = reader.ReadString();
                    DirectConnect = reader.ReadBool();
                    break;

                case LobbyOpcode.LobbyKick:
                    Id = reader.ReadByte();
                    break;

                case LobbyOpcode.LobbyReady:
                    Ready = reader.ReadBool();
                    break;
            }
        }

        private GameServer Server { get; set; }

        public override void Handle(Peer peer)
        {
            Server = NetworkManager.GameServer;

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

                case LobbyOpcode.LobbyKick:
                    HandleKick(peer);
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

        // LobbyKick
        public byte Id { get; set; }
        private void HandleKick(Peer peer)
        {
            if (!Server.Players[(byte)peer.ID].Host)
                return;

            Server.Kick(Id, DisconnectOpcode.Kicked);
        }

        // LobbyCreate
        public string LobbyName { get; set; }
        public string LobbyDescription { get; set; }
        private void HandleCreate(Peer peer)
        {
            Server.Lobby = new DataLobby {
                Name = LobbyName,
                Description = LobbyDescription,
                HostId = (byte)peer.ID
            };

            Server.Players[(byte)peer.ID] = new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = true
            };

            Server.Send(ServerPacketOpcode.Lobby, new SPacketLobby {
                LobbyOpcode = LobbyOpcode.LobbyCreate,
                Id = (byte)peer.ID
            }, peer);
        }

        // LobbyChatMessage
        public string Message { get; set; }

        private void HandleChatMessage(Peer peer)
        {
            Server.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby
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
            if (!Server.Players[(byte)peer.ID].Host)
                return;

            Server.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyCountdownChange,
                CountdownRunning = CountdownRunning
            });
        }

        // LobbyGameStart
        private void HandleGameStart(Peer peer)
        {
            if (!Server.Players[(byte)peer.ID].Host)
                return;

            Server.DisallowJoiningLobby = true;
            Server.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby { LobbyOpcode = LobbyOpcode.LobbyGameStart });
        }

        // LobbyJoin
        public string Username { get; set; }
        public bool DirectConnect { get; set; }
        private void HandleJoin(Peer peer)
        {
            // Check if data.Username is appropriate username
            // TODO

            // Keep track of joining player server side
            if (Server.Players.ContainsKey((byte)peer.ID))
            {
                Server.Log($"Received LobbyJoin packet from peer with id {peer.ID}. Tried to add id {peer.ID} to Players but exists already");
                return;
            }

            if (Server.DisallowJoiningLobby) 
            {
                Server.Kick(peer.ID, DisconnectOpcode.Disconnected);
                Server.Log($"Peer with id {peer.ID} tried to join lobby but game is running already");
                return;
            }

            Server.Players[(byte)peer.ID] = new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = false
            };

            // tell joining player their Id and tell them about other players in lobby
            Server.Send(ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyInfo,
                Id = (byte)peer.ID,
                Players = Server.GetOtherPlayers((byte)peer.ID),
                DirectConnect = DirectConnect,
                LobbyName = Server.Lobby.Name,
                LobbyDescription = Server.Lobby.Description,
                LobbyHostId = Server.Lobby.HostId,
                LobbyMaxPlayerCount = Server.MaxPlayers
            }, peer);

            // tell other players about new player that joined
            Server.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
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
            var player = Server.Players[(byte)peer.ID];
            player.Ready = Ready;

            Server.SendToOtherPlayers(peer.ID, ServerPacketOpcode.Lobby, new SPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyReady,
                Id = (byte)peer.ID,
                Ready = Ready
            });
        }
    }
}