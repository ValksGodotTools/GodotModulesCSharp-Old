using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode
{
    public class SPacketLobby : APacketServerPeerId
    {
        public LobbyOpcode LobbyOpcode { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)LobbyOpcode);

            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    base.Write(writer);
                    writer.Write((string)Message);
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    writer.Write(CountdownRunning);
                    break;

                case LobbyOpcode.LobbyInfo:
                    base.Write(writer);
                    writer.Write(IsHost);
                    writer.Write((ushort)Players.Count);
                    Players.ForEach(pair =>
                    {
                        writer.Write((ushort)pair.Key); // id
                        writer.Write((string)pair.Value.Username);
                    });
                    break;

                case LobbyOpcode.LobbyJoin:
                    base.Write(writer);
                    writer.Write((string)Username);
                    break;

                case LobbyOpcode.LobbyLeave:
                    base.Write(writer);
                    break;

                case LobbyOpcode.LobbyReady:
                    base.Write(writer);
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
                    base.Read(reader);
                    Message = reader.ReadString();
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    CountdownRunning = reader.ReadBool();
                    break;

                case LobbyOpcode.LobbyInfo:
                    base.Read(reader);
                    IsHost = reader.ReadBool();
                    var count = reader.ReadUShort();
                    Players = new Dictionary<uint, DataPlayer>();
                    for (int i = 0; i < count; i++)
                    {
                        var id = reader.ReadUShort();
                        var name = reader.ReadString();

                        Players.Add(id, new DataPlayer
                        {
                            Username = name,
                            Ready = false
                        });
                    }
                    break;

                case LobbyOpcode.LobbyJoin:
                    base.Read(reader);
                    Username = reader.ReadString();
                    break;

                case LobbyOpcode.LobbyLeave:
                    base.Read(reader);
                    break;

                case LobbyOpcode.LobbyReady:
                    base.Read(reader);
                    Ready = reader.ReadBool();
                    break;
            }
        }

        public override void Handle()
        {
            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyChatMessage:
                    HandleChatMessage();
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    HandleCountdownChange();
                    break;

                case LobbyOpcode.LobbyGameStart:
                    HandleGameStart();
                    break;

                case LobbyOpcode.LobbyInfo:
                    HandleInfo();
                    break;

                case LobbyOpcode.LobbyJoin:
                    HandleJoin();
                    break;

                case LobbyOpcode.LobbyLeave:
                    HandleLeave();
                    break;

                case LobbyOpcode.LobbyReady:
                    HandleReady();
                    break;
            }
        }

        // LobbyChatMessage
        public string Message { get; set; }

        private void HandleChatMessage()
        {
            SceneLobby.Log(Id, Message);
        }

        // LobbyCountdownChange
        public bool CountdownRunning { get; set; }

        private void HandleCountdownChange()
        {
            if (CountdownRunning)
                SceneLobby.StartGameCountdown();
            else
                SceneLobby.CancelGameCountdown();
        }

        // LobbyGameStart
        private void HandleGameStart()
        {
            if (GameClient.IsHost)
                GameServer.EmitClientPositions.Enabled = true;

            SceneManager.ChangeScene("Game");
        }

        // LobbyInfo
        public bool IsHost { get; set; }

        public Dictionary<uint, DataPlayer> Players { get; set; }

        private void HandleInfo()
        {
            GameClient.PeerId = Id;
            GameClient.IsHost = IsHost;
            GameClient.Log($"{GameManager.Options.OnlineUsername} joined lobby with id {Id}");
            GameClient.Players.Add(Id, GameManager.Options.OnlineUsername);
            Players.ForEach(pair => GameClient.Players.Add(pair.Key, pair.Value.Username));

            SceneManager.ChangeScene("Lobby");
        }

        // LobbyJoin
        public string Username { get; set; }

        private void HandleJoin()
        {
            SceneLobby.AddPlayer(Id, Username);
            GameClient.Players.Add(Id, Username);

            GameClient.Log($"Player with username {Username} id: {Id} joined the lobby");
        }

        // LobbyLeave
        private void HandleLeave()
        {
            SceneLobby.RemovePlayer(Id);
            GameClient.Players.Remove(Id);
            GameClient.Log($"Player with id: {Id} left the lobby");
        }

        // LobbyReady
        public bool Ready { get; set; }

        private void HandleReady()
        {
            SceneLobby.SetReady(Id, Ready);
        }
    }
}