using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules
{
    public class SceneLobby : AScene
    {
        public static LobbyListing CurrentLobby { get; set; }

        [Export] public readonly NodePath NodePathPlayers;
        [Export] public readonly NodePath NodePathChatText;
        [Export] public readonly NodePath NodePathChatInput;
        [Export] public readonly NodePath NodePathLobbyName;
        [Export] public readonly NodePath NodePathLobbyMaxPlayers;
        [Export] public readonly NodePath NodePathBtnReady;
        [Export] public readonly NodePath NodePathBtnStart;

        private RichTextLabel ChatText { get; set; }
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
            ListPlayers = GetNode<Control>(NodePathPlayers);
            ChatText = GetNode<RichTextLabel>(NodePathChatText);
            ChatInput = GetNode<LineEdit>(NodePathChatInput);
            LobbyName = GetNode<Label>(NodePathLobbyName);
            LobbyMaxPlayers = GetNode<Label>(NodePathLobbyMaxPlayers);
            BtnReady = GetNode<Button>(NodePathBtnReady);
            BtnStart = GetNode<Button>(NodePathBtnStart);
            UIPlayers = new();
            TimerCountdownGameStart = new STimer(1000, TimerCountdownCallback, false);

            if (!NetworkManager.IsHost)
                BtnStart.Disabled = true;

            NetworkManager.GameClient.Players.ForEach(x => AddPlayer(x.Key, x.Value));

            LobbyName.Text = CurrentLobby.Name;
            LobbyMaxPlayers.Text = "" + CurrentLobby.MaxPlayerCount;
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel")) 
            {
                WebClient.Client.CancelPendingRequests();
                WebClient.TimerPingMasterServer.Stop();
                NetworkManager.GameClient.Stop();
                if (NetworkManager.GameServer != null)
                    NetworkManager.GameServer.Stop();
            }
        }

        private async void TimerCountdownCallback()
        {
            Log($"Game starting in {CountdownGameStart--}");

            if (CountdownGameStart == 0)
            {
                TimerCountdownGameStart.Stop();

                if (NetworkManager.IsHost)
                {
                    WebClient.TimerPingMasterServer.Stop();

                    // tell everyone game has started
                    await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                    {
                        LobbyOpcode = LobbyOpcode.LobbyGameStart
                    });
                }
            }
        }

        public void AddPlayer(uint id, string name)
        {
            if (UIPlayers.Duplicate(id))
                return;

            var player = Prefabs.LobbyPlayerListing.Instance<UILobbyPlayerListing>();
            UIPlayers[id] = player;

            ListPlayers.AddChild(player);

            player.SetUsername(name);
            player.SetReady(false);
        }

        public void RemovePlayer(uint id)
        {
            if (UIPlayers.DoesNotHave(id))
                return;
            
            var uiPlayer = UIPlayers[id];
            uiPlayer.QueueFree();
            UIPlayers.Remove(id);
        }

        public void SetReady(uint id, bool ready)
        {
            if (UIPlayers.DoesNotHave(id))
                return;

            var player = UIPlayers[id];
            player.SetReady(ready);
        }

        public void CancelGameCountdown()
        {
            TimerCountdownGameStart.Stop();
            CountdownGameStart = COUNTDOWN_START_TIME;
            BtnReady.Disabled = false;
            Log("Game start was cancelled");
        }

        public void StartGameCountdown()
        {
            TimerCountdownGameStart.Start();
            BtnReady.Disabled = true;
        }

        public void Log(string text)
        {
            ChatText.AddText($"{text}\n");
            ChatText.ScrollToLine(ChatText.GetLineCount() - 1);
        }

        public void Log(uint peerId, string text)
        {
            if (UIPlayers.DoesNotHave(peerId))
                return;

            var playerName = NetworkManager.GameClient.Players[peerId];
            Log($"{playerName}: {text}");
        }

        private async void _on_Ready_pressed()
        {
            var player = UIPlayers[NetworkManager.PeerId];
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
            if (!NetworkManager.IsHost)
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

        public override void Cleanup()
        {
            TimerCountdownGameStart.Dispose();
        }
    }
}