using LiteNetLib;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode
{
    public class CPacketLobby : APacketClient
    {
        private LobbyOpcode LobbyOpcode { get; set; }

        // LobbyKick
        public byte Id { get; set; }

        // LobbyCreate
        public string LobbyName { get; set; }
        public string LobbyDescription { get; set; }

        // LobbyChatMessage
        public string Message { get; set; }

        // LobbyCountdownChange
        public bool CountdownRunning { get; set; }

        // LobbyJoin
        public string Username { get; set; }
        public bool DirectConnect { get; set; }

        // LobbyReady
        public bool Ready { get; set; }

        public CPacketLobby() {} // required because of ReflectionUtils

        public CPacketLobby(LobbyOpcode opcode)
        {
            LobbyOpcode = opcode;
        }

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

        private GameServer _server;

        public override void Handle(GameServer server, NetPeer peer)
        {
            _server = server;
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

        private void HandleKick(NetPeer peer)
        {
            if (!_server.Players[(byte)peer.Id].Host)
                return;

            _server.Kick(Id, DisconnectOpcode.Kicked);
        }

        private void HandleCreate(NetPeer peer)
        {
            _server.Lobby = new DataLobby {
                Name = LobbyName,
                Description = LobbyDescription,
                HostId = (byte)peer.Id
            };

            _server.Players[(byte)peer.Id] = new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = true
            };

            _server.Send(ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyCreate) {
                Id = (byte)peer.Id
            }, peer);
        }

        private void HandleChatMessage(NetPeer peer)
        {
            _server.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyChatMessage)
            {
                Id = (byte)peer.Id,
                Message = Message
            });
        }

        private void HandleCountdownChange(NetPeer peer)
        {
            if (!_server.Players[(byte)peer.Id].Host)
                return;

            _server.SendToOtherPlayers(peer.Id, ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyCountdownChange)
            {
                CountdownRunning = CountdownRunning
            });
        }

        private void HandleGameStart(NetPeer peer)
        {
            if (!_server.Players[(byte)peer.Id].Host)
                return;

            _server.Lobby.AllowJoining = false;
            _server.SendToAllPlayers(ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyGameStart));
        }

        private void HandleJoin(NetPeer peer)
        {
            // Check if data.Username is appropriate username
            // TODO

            // Keep track of joining player server side
            if (_server.Players.ContainsKey((byte)peer.Id))
            {
                _server.Log($"Received LobbyJoin packet from peer with id {peer.Id}. Tried to add id {peer.Id} to Players but exists already");
                return;
            }

            if (!_server.Lobby.AllowJoining) 
            {
                _server.Kick(peer.Id, DisconnectOpcode.Disconnected);
                _server.Log($"Peer with id {peer.Id} tried to join lobby but game is running already");
                return;
            }

            _server.Players[(byte)peer.Id] = new DataPlayer
            {
                Username = Username,
                Ready = false,
                Host = false
            };

            // tell joining player their Id and tell them about other players in lobby
            _server.Send(ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyInfo)
            {
                Id = (byte)peer.Id,
                Players = _server.GetOtherPlayers((byte)peer.Id),
                DirectConnect = DirectConnect,
                LobbyName = _server.Lobby.Name,
                LobbyDescription = _server.Lobby.Description,
                LobbyHostId = _server.Lobby.HostId,
                LobbyMaxPlayerCount = _server.Lobby.MaxPlayerCount
            }, peer);

            // tell other players about new player that joined
            _server.SendToOtherPlayers(peer.Id, ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyJoin)
            {
                Id = (byte)peer.Id,
                Username = Username
            });
        }

        private void HandleReady(NetPeer peer)
        {
            var player = _server.Players[(byte)peer.Id];
            player.Ready = Ready;

            _server.SendToOtherPlayers(peer.Id, ServerPacketOpcode.Lobby, new SPacketLobby(LobbyOpcode.LobbyReady)
            {
                Id = (byte)peer.Id,
                Ready = Ready
            });
        }
    }
}