using Godot;
using System.Collections.Generic;
using GodotModules.Settings;

namespace GodotModules.Netcode
{
    public class UILobby : Node
    {
        [Export] public readonly NodePath NodePathPlayers;
        [Export] public readonly NodePath NodePathChatText;
        [Export] public readonly NodePath NodePathChatInput;
        [Export] public readonly NodePath NodePathLobbyName;
        [Export] public readonly NodePath NodePathLobbyMaxPlayers;
        [Export] public readonly NodePath NodePathBtnReady;
        [Export] public readonly NodePath NodePathBtnStart;

        private Control ListPlayers { get; set; }
        private RichTextLabel ChatText { get; set; }
        private LineEdit ChatInput { get; set; }
        private Label LobbyName { get; set; }
        private Label LobbyMaxPlayers { get; set; }
        private Button BtnReady { get; set; }
        private Button BtnStart { get; set; }

        public bool Ready { get; set; }
        public bool Start { get; set; }

        private System.Threading.Timer TimerCountdownGameStart { get; set; }
        private int CountdownGameStart = 5;

        public static Dictionary<uint, string> Players = new Dictionary<uint, string>();
        private Dictionary<uint, UILobbyPlayerListing> UIPlayers { get; set; }
        private static UILobby Instance { get; set; }
        public static LobbyListing CurrentLobby { get; set; }

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

            UIPlayers = new Dictionary<uint, UILobbyPlayerListing>();

            //var info = UIGameServers.Instance.CurrentLobby;
            //LobbyName.Text = info.Name;
            //LobbyMaxPlayers.Text = "" + info.MaxPlayerCount;

            foreach (var player in Players)
                UIAddPlayer(player.Key, player.Value);
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GameManager.WebClient.Client.CancelPendingRequests();
                GameManager.WebClient.TimerPingMasterServer.Stop();
                GameManager.GameClient.Disconnect();
                GameManager.GameServer.Stop();
                GameManager.ChangeScene("GameServers");
            }
        }

        public static void AddPlayer(uint id, string name) 
        {
            Players.Add(id, name);

            if (GameManager.ActiveScene == "Lobby")
                Instance.UIAddPlayer(id, name);
        }

        public void UIAddPlayer(uint id, string name)
        {
            var playerPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyPlayerListing.tscn");
            var player = playerPrefab.Instance<UILobbyPlayerListing>();
            player.Init();
            player.SetInfo(new LobbyPlayerListing
            {
                Name = name,
                Ready = false
            });

            UIPlayers.Add(id, player);
            ListPlayers.AddChild(player);
        }

        private void _on_Ready_pressed()
        {
            /*var player = UIPlayers[ClientUsername];
            var info = player.Info;
            info.Ready = !info.Ready;
            player.Info = info;
            player.Status.Text = info.Ready ? "Ready" : "Not Ready";

            BtnReady.Text = info.Ready ? "Ready" : "Not Ready";*/
        }

        private void _on_Start_pressed()
        {
            // check if all players are ready
            /*var playersNotReady = new List<string>();
            foreach (var pair in UIPlayers)
                if (!pair.Value.Info.Ready)
                    playersNotReady.Add(pair.Key);

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
                TimerCountdown = new System.Threading.Timer(TimerCountdownCallback, "state", 0, 1000);
                BtnReady.Disabled = true;
            }
            else
            {
                TimerCountdown.Dispose();
                Countdown = 5;
                Log("Game start was cancelled");
                BtnReady.Disabled = false;
            }*/
        }

        private void TimerCountdownCallback(object state)
        {
            Log($"Game starting in {CountdownGameStart--}");

            if (CountdownGameStart == 0)
            {
                TimerCountdownGameStart.Dispose();
                // load game scene
            }
        }

        private void Log(string text) => ChatText.AddText($"{text}\n");

        private void _on_Chat_Input_text_entered(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                Log(text.Trim());
            ChatInput.Clear();
        }
    }
}