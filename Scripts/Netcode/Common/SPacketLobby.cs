using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using Game;
using System.Threading.Tasks;

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
                    writer.Write((byte)Players.Count);
                    Players.ForEach(pair =>
                    {
                        writer.Write((byte)pair.Key); // id
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
                    var count = reader.ReadByte();
                    Players = new Dictionary<byte, DataPlayer>();
                    for (int i = 0; i < count; i++)
                    {
                        var id = reader.ReadByte();
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

        public override async Task Handle(ENetClient client)
        {
            switch (LobbyOpcode)
            {
                case LobbyOpcode.LobbyCreate:
                    await HandleCreate();
                    break;

                case LobbyOpcode.LobbyInfo:
                    await HandleInfo();
                    break;

                case LobbyOpcode.LobbyJoin:
                    HandleJoin();
                    break;

                case LobbyOpcode.LobbyChatMessage:
                    HandleChatMessage();
                    break;

                case LobbyOpcode.LobbyLeave:
                    HandleLeave();
                    break;

                case LobbyOpcode.LobbyReady:
                    HandleReady();
                    break;

                case LobbyOpcode.LobbyCountdownChange:
                    HandleCountdownChange();
                    break;

                case LobbyOpcode.LobbyGameStart:
                    await HandleGameStart();
                    break;
            }
        }

        // LobbyCreate
        private async Task HandleCreate()
        {
            NetworkManager.GameClient.PeerId = Id;
            NetworkManager.GameClient.IsHost = true;
            NetworkManager.GameClient.Log($"{GameManager.Options.OnlineUsername} created lobby with their id being {Id}");
            NetworkManager.GameClient.Players.Add(Id, GameManager.Options.OnlineUsername);

            await SceneManager.ChangeScene("Lobby");
        }

        // LobbyChatMessage
        public string Message { get; set; }

        private void HandleChatMessage()
        {
            if (SceneManager.InLobby())
                SceneManager.GetActiveSceneScript<SceneLobby>().Log(Id, Message);
        }

        // LobbyCountdownChange
        public bool CountdownRunning { get; set; }

        private void HandleCountdownChange()
        {
            if (!SceneManager.InLobby())
                return;

            if (CountdownRunning)
                SceneManager.GetActiveSceneScript<SceneLobby>().StartGameCountdown();
            else
                SceneManager.GetActiveSceneScript<SceneLobby>().CancelGameCountdown();
        }

        // LobbyGameStart
        private async Task HandleGameStart()
        {
            if (NetworkManager.GameClient.IsHost)
                NetworkManager.GameServer.StartGame();

            await SceneManager.ChangeScene("Game");
        }

        // LobbyInfo
        public Dictionary<byte, DataPlayer> Players { get; set; }

        private async Task HandleInfo()
        {
            NetworkManager.GameClient.PeerId = Id;
            NetworkManager.GameClient.Log($"{GameManager.Options.OnlineUsername} joined lobby with id {Id}");
            NetworkManager.GameClient.Players.Add(Id, GameManager.Options.OnlineUsername);
            Players.ForEach(pair => NetworkManager.GameClient.Players.Add(pair.Key, pair.Value.Username));

            await SceneManager.ChangeScene("Lobby");
        }

        // LobbyJoin
        public string Username { get; set; }

        private void HandleJoin()
        {
            if (SceneManager.InLobby())
                SceneManager.GetActiveSceneScript<SceneLobby>().AddPlayer(Id, Username);
            NetworkManager.GameClient.Players.Add(Id, Username);

            NetworkManager.GameClient.Log($"Player with username {Username} id: {Id} joined the lobby");
        }

        // LobbyLeave
        private void HandleLeave()
        {
            if (SceneManager.InLobby())
                SceneManager.GetActiveSceneScript<SceneLobby>().RemovePlayer(Id);

            if (SceneManager.InGame())
            {
                SceneManager.GetActiveSceneScript<SceneGame>().RemovePlayer(Id);
            }
            
            NetworkManager.GameClient.Players.Remove(Id);
            NetworkManager.GameClient.Log($"Player with id: {Id} left the lobby");
        }

        // LobbyReady
        public bool Ready { get; set; }

        private void HandleReady()
        {
            if (SceneManager.InLobby())
                SceneManager.GetActiveSceneScript<SceneLobby>().SetReady(Id, Ready);
        }
    }
}