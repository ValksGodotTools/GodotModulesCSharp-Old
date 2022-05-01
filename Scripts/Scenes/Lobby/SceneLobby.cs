using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules
{
    public class SceneLobby : Node
    {
        public static LobbyListing CurrentLobby { get; set; }
        private static SceneLobby Instance { get; set; }

        [Export] public readonly NodePath NodePathPlayers;
        [Export] public readonly NodePath NodePathChatText;
        [Export] public readonly NodePath NodePathChatInput;
        [Export] public readonly NodePath NodePathLobbyName;
        [Export] public readonly NodePath NodePathLobbyMaxPlayers;
        [Export] public readonly NodePath NodePathBtnReady;
        [Export] public readonly NodePath NodePathBtnStart;

        private static RichTextLabel ChatText { get; set; }
        private Control ListPlayers { get; set; }
        private LineEdit ChatInput { get; set; }
        private Label LobbyName { get; set; }
        private Label LobbyMaxPlayers { get; set; }
        private Button BtnReady { get; set; }
        private Button BtnStart { get; set; }

        public bool Start { get; set; }

        private const int COUNTDOWN_START_TIME = 2;
        private int CountdownGameStart = COUNTDOWN_START_TIME;
        private STimer TimerCountdownGameStart { get; set; }

        private Dictionary<uint, UILobbyPlayerListing> UIPlayers { get; set; }

        public override void _Ready()
        {
            Instance = this;
            ListPlayers = GetNode<Control>(NodePathPlayers);
            ChatText = GetNode<RichTextLabel>(NodePathChatText);
            ChatInput = GetNode<LineEdit>(NodePathChatInput);
            LobbyName = GetNode<Label>(NodePathLobbyName);
            LobbyMaxPlayers = GetNode<Label>(NodePathLobbyMaxPlayers);
            BtnReady = GetNode<Button>(NodePathBtnReady);
            BtnStart = GetNode<Button>(NodePathBtnStart);
            UIPlayers = new();

            if (!NetworkManager.GameClient.IsHost)
                Instance.BtnStart.Disabled = true;

            NetworkManager.GameClient.Players.ForEach(x => AddPlayer(x.Key, x.Value));
        }

        public override void _Input(InputEvent @event)
        {
            Utils.EscapeToScene("GameServers", async () =>
            {
                WebClient.Client.CancelPendingRequests();
                WebClient.TimerPingMasterServer.Stop();
                await NetworkManager.GameClient.Stop();
                if (NetworkManager.GameServer.Running)
                    await NetworkManager.GameServer.Stop();
            });
        }

        public static void AddPlayer(uint id, string name)
        {
            var player = Prefabs.LobbyPlayerListing.Instance<UILobbyPlayerListing>();
            Instance.UIPlayers.Add(id, player);

            Instance.ListPlayers.AddChild(player);

            player.SetUsername(name);
            player.SetReady(false);
        }

        public static void RemovePlayer(uint id)
        {
            var uiPlayer = Instance.UIPlayers[id];
            uiPlayer.QueueFree();
            Instance.UIPlayers.Remove(id);
        }

        public static void SetReady(uint id, bool ready)
        {
            var player = Instance.UIPlayers[id];
            player.SetReady(ready);
        }

        public static void CancelGameCountdown()
        {
            Instance.TimerCountdownGameStart.Dispose();
            Instance.CountdownGameStart = COUNTDOWN_START_TIME;
            Instance.BtnReady.Disabled = false;
            Log("Game start was cancelled");
        }

        public static void StartGameCountdown()
        {
            Instance.TimerCountdownGameStart = new STimer(1000, Instance.TimerCountdownCallback);
            Instance.BtnReady.Disabled = true;
        }

        private async void TimerCountdownCallback()
        {
            Log($"Game starting in {CountdownGameStart--}");

            if (CountdownGameStart == 0)
            {
                TimerCountdownGameStart.Dispose();

                if (NetworkManager.GameClient.IsHost)
                {
                    // tell everyone game has started
                    await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                    {
                        LobbyOpcode = LobbyOpcode.LobbyGameStart
                    });
                }
            }
        }

        public static void Log(string text)
        {
            ChatText.AddText($"{text}\n");
            ChatText.ScrollToLine(ChatText.GetLineCount() - 1);
        }

        public static void Log(uint peerId, string text)
        {
            var playerName = NetworkManager.GameClient.Players[peerId];
            Log($"{playerName}: {text}");
        }

        private async void _on_Ready_pressed()
        {
            var player = UIPlayers[NetworkManager.GameClient.PeerId];
            player.SetReady(!player.Ready);

            BtnReady.Text = player.Ready ? "Ready" : "Not Ready";

            await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
            {
                LobbyOpcode = LobbyOpcode.LobbyReady,
                Ready = player.Ready
            });
        }

        private async void _on_Start_pressed()
        {
            if (!NetworkManager.GameClient.IsHost)
                return;

            // check if all players are ready
            var playersNotReady = new List<string>();
            UIPlayers.Where(x => !x.Value.Ready).ForEach(pair => playersNotReady.Add(pair.Value.Username));

            if (playersNotReady.Count > 0)
            {
                var isAre = playersNotReady.Count == 1 ? "is" : "are";
                Log($"Cannot start because {string.Join(" ", playersNotReady.ToArray())} {isAre} not ready");
                return;
            }

            Start = !Start;
            BtnStart.Text = Start ? "Cancel" : "Start";

            if (Start)
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyCountdownChange,
                    CountdownRunning = true
                });
                StartGameCountdown();
            }
            else
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyCountdownChange,
                    CountdownRunning = false
                });
                CancelGameCountdown();
            }
        }

        private async void _on_Chat_Input_text_entered(string text)
        {
            ChatInput.Clear();
            if (!string.IsNullOrWhiteSpace(text))
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyChatMessage,
                    Message = text.Trim()
                });
            }
        }
    }
}