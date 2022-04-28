using Godot;
using GodotModules;
using GodotModules.ModLoader;
using GodotModules.Netcode.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class SceneGame : Node
    {
        public static ClientPlayer Player { get; set; }
        public static SceneGame Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public Label LabelPlayerHealth;

        private Dictionary<uint, OtherPlayer> Players;
        public static Vector2 ServerPlayerPosition = Vector2.Zero;
        private static Dictionary<uint, Vector2> NextServerPlayerPositions { get; set; }
        private static Dictionary<uint, Vector2> PrevServerPlayerPositions { get; set; }

        public override void _Ready()
        {
            Players = new();
            NextServerPlayerPositions = new();
            PrevServerPlayerPositions = new();
            Instance = this;
            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            Player = Prefabs.ClientPlayer.Instance<ClientPlayer>();
            Player.Position = Vector2.Zero;
            Players.Add(GameClient.PeerId, Player);
            AddChild(Player);

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)SceneGame.Player.SetHealth;

            ModLoader.Call("OnGameInit");

            if (GameClient.Running)
                InitMultiplayerStuff();
        }

        private void InitMultiplayerStuff()
        {
            Player.SetUsername(GameManager.Options.OnlineUsername);

            bool IsNotClient(uint id) => id != GameClient.PeerId;

            GameClient.Players
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

        private static List<Dictionary<uint, Vector2>> PlayerPositionQueue = new List<Dictionary<uint, Vector2>>();

        public static void UpdatePlayerPositions(Dictionary<uint, Vector2> playerPositions)
        {
            if (SceneManager.ActiveScene != "Game")
                return;

            PlayerPositionQueue.Add(playerPositions);

            if (PlayerPositionQueue.Count > 2)
                PlayerPositionQueue.RemoveAt(0);

            progress = 0;
        }

        private static float progress = 0.0f;

        public override void _PhysicsProcess(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);

            progress += delta * (1000f / CommandDebug.TestValue1); // reach value of 1.0 every 200ms

            if (PlayerPositionQueue.Count <= 1)
            {
                PrevServerPlayerPositions = NextServerPlayerPositions;
                return;
            }

            PlayerPositionQueue[1].ForEach(pair => {
                var player = Players[pair.Key];
                player.Position = Utils.Lerp(PlayerPositionQueue[0][pair.Key], pair.Value, progress);
            });
        }
    }
}