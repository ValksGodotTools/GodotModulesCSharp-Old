using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GodotModules
{
    public class SceneLobby : Node
    {
        private static PackedScene PrefabLobbyPlayerListing = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyPlayerListing.tscn");
        public static LobbyListing CurrentLobby { get; set; }
        private static SceneLobby Instance { get; set; }
        
        [Export] public readonly NodePath NodePathPlayers;
        [Export] public readonly NodePath NodePathChatText;
        [Export] public readonly NodePath NodePathChatInput;
        [Export] public readonly NodePath NodePathLobbyName;
        [Export] public readonly NodePath NodePathLobbyMaxPlayers;
        [Export] public readonly NodePath NodePathBtnReady;
        [Export] public readonly NodePath NodePathBtnStart;

        private Control ListPlayers { get; set; }
        private static RichTextLabel ChatText { get; set; }
        private LineEdit ChatInput { get; set; }
        private Label LobbyName { get; set; }
        private Label LobbyMaxPlayers { get; set; }
        private Button BtnReady { get; set; }
        private Button BtnStart { get; set; }

        public bool Ready { get; set; }
        public bool Start { get; set; }

        private System.Threading.Timer TimerCountdownGameStart { get; set; }
        private int CountdownGameStart = 5;

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

            UIPlayers = new Dictionary<uint, UILobbyPlayerListing>();

            //var info = UIGameServers.Instance.CurrentLobby;
            //LobbyName.Text = info.Name;
            //LobbyMaxPlayers.Text = "" + info.MaxPlayerCount;

            foreach (var player in NetworkManager.GameClient.Players)
                UIAddPlayer(player.Key, player.Value);
        }

        public override void _Input(InputEvent @event)
        {
            Utils.EscapeToScene("GameServers", async () => {
                NetworkManager.WebClient.Client.CancelPendingRequests();
                NetworkManager.WebClient.TimerPingMasterServer.Stop();
                await GameClient.Stop();
                if (ENetServer.Running)
                    await GameServer.Stop();
            });
        }

        public static void AddPlayer(uint id, string name) 
        {
            if (NetworkManager.GameClient.Players.ContainsKey(id))
            {
                GD.Print($"Players contains id: '{id}' already");
                GD.Print(NetworkManager.GameClient.Players.Print());
                return;
            }

            NetworkManager.GameClient.Players.Add(id, name);

            if (SceneManager.ActiveScene == "Lobby")
                Instance.UIAddPlayer(id, name);
        }

        public static void RemovePlayer(uint id)
        {
            NetworkManager.GameClient.Players.Remove(id);

            if (SceneManager.ActiveScene == "Lobby")
            {
                var uiPlayer = Instance.UIPlayers[id];
                uiPlayer.QueueFree();
                Instance.UIPlayers.Remove(id);
            }
        }

        public void UIAddPlayer(uint id, string name)
        {
            var player = PrefabLobbyPlayerListing.Instance<UILobbyPlayerListing>();
            UIPlayers.Add(id, player);
            
            ListPlayers.AddChild(player);

            player.SetUsername(name);
            player.SetReady(false);
        }

        private async void _on_Ready_pressed()
        {
            var player = UIPlayers[ENetClient.PeerId];
            player.SetReady(!player.Ready);

            BtnReady.Text = player.Ready ? "Ready" : "Not Ready";

            await ENetClient.Send(ClientPacketOpcode.LobbyReady, new CPacketLobbyReady {
                Ready = player.Ready
            });
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

        public static void Log(string text) => ChatText.AddText(text);

        public static void Log(uint peerId, string text) 
        {
            if (SceneManager.ActiveScene != "Lobby")
                return;

            var playerName = NetworkManager.GameClient.Players[peerId];
            Log($"{playerName}: {text}\n");
        }

        private async void _on_Chat_Input_text_entered(string text)
        {
            ChatInput.Clear();
            if (!string.IsNullOrWhiteSpace(text))
            {
                await GameClient.Send(ClientPacketOpcode.LobbyChatMessage, new CPacketLobbyChatMessage {
                    Message = text.Trim()
                });
            }
        }
    }
}