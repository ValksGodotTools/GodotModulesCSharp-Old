using Godot;
using GodotModules.ModLoader;
using GodotModules.Netcode.Client;
using System;

namespace Game
{
    public class SceneGame : Node
    {
        public static SceneGame Instance { get; set; }
        private static PositionQueue PlayerPositionQueue = new PositionQueue();
        public static void UpdatePlayerPositions(Dictionary<byte, Vector2> playerPositions)
        {
            PlayerPositionQueue.Add(playerPositions);
        }

        public ClientPlayer Player { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public Label LabelPlayerHealth;

        private Dictionary<uint, OtherPlayer> Players;
        private Dictionary<uint, Vector2> NextServerPlayerPositions { get; set; }
        private Dictionary<uint, Vector2> PrevServerPlayerPositions { get; set; }

        public override void _Ready()
        {
            Players = new();
            NextServerPlayerPositions = new();
            PrevServerPlayerPositions = new();
            Instance = this;
            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            Player = Prefabs.ClientPlayer.Instance<ClientPlayer>();
            Player.Position = Vector2.Zero;
            Players.Add(NetworkManager.GameClient.PeerId, Player);
            AddChild(Player);

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)Player.SetHealth;

            ModLoader.Call("OnGameInit");

            if (NetworkManager.GameClient.Running)
                InitMultiplayerStuff();
        }

        public override async void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel")) 
            {
                NetworkManager.GameClient?.Stop();
                await NetworkManager.GameServer?.Stop();
            }
        }

        private void InitMultiplayerStuff()
        {
            Player.SetUsername(GameManager.Options.OnlineUsername);

            bool IsNotClient(uint id) => id != NetworkManager.GameClient.PeerId;

            NetworkManager.GameClient.Players
                .Where(x => IsNotClient(x.Key))
                .ForEach(pair =>
                {
                    var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
                    otherPlayer.Position = Vector2.Zero;
                    Players.Add(pair.Key, otherPlayer);
                    AddChild(otherPlayer);
                    otherPlayer.SetUsername(pair.Value);
                });
        }

        public override void _PhysicsProcess(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);

            PlayerPositionQueue.UpdateProgress(delta);

            if (PlayerPositionQueue.NotReady)
            {
                PrevServerPlayerPositions = NextServerPlayerPositions;
                return;
            }

            PlayerPositionQueue.Current.ForEach(pair =>
            {
                var player = Players[pair.Key];
                player.Position = Utils.Lerp(PlayerPositionQueue.Previous[pair.Key], pair.Value, PlayerPositionQueue.Progress);
            });
        }
    }
}