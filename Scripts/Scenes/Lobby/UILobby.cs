using Godot;
using System.Collections.Generic;

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

        private Control Players { get; set; }
        private RichTextLabel ChatText { get; set; }
        private LineEdit ChatInput { get; set; }
        private Label LobbyName { get; set; }
        private Label LobbyMaxPlayers { get; set; }
        private Button BtnReady { get; set; }
        private Button BtnStart { get; set; }

        public bool Ready { get; set; }
        public bool Start { get; set; }

        private System.Threading.Timer TimerCountdown { get; set; }
        private int Countdown = 5;

        public override void _Ready()
        {
            Players = GetNode<Control>(NodePathPlayers);
            ChatText = GetNode<RichTextLabel>(NodePathChatText);
            ChatInput = GetNode<LineEdit>(NodePathChatInput);
            LobbyName = GetNode<Label>(NodePathLobbyName);
            LobbyMaxPlayers = GetNode<Label>(NodePathLobbyMaxPlayers);
            BtnReady = GetNode<Button>(NodePathBtnReady);
            BtnStart = GetNode<Button>(NodePathBtnStart);

            var info = UIGameServers.Instance.CurrentLobby;
            LobbyName.Text = info.Name;
            LobbyMaxPlayers.Text = "" + info.MaxPlayerCount;
        }

        public void AddPlayer(string name)
        {
            var playerPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyPlayerListing.tscn");
            var player = playerPrefab.Instance<UILobbyPlayerListing>();
            player.Init();

            var info = new LobbyPlayerListing
            {
                Name = name,
                Ready = false
            };

            player.SetInfo(info);
            Players.AddChild(player);
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
            Log($"Game starting in {Countdown--}");

            if (Countdown == 0)
            {
                TimerCountdown.Dispose();
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