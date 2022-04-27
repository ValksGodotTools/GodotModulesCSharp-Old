using Godot;
using GodotModules;
using GodotModules.ModLoader;
using GodotModules.Netcode.Client;
using System;
using System.Collections.Generic;

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
        public static Dictionary<uint, Vector2> ServerPlayerPositions { get; set; }

        public override void _Ready()
        {
            Players = new Dictionary<uint, OtherPlayer>();
            ServerPlayerPositions = new Dictionary<uint, Vector2>();
            ServerPlayerPositions.Add(GameClient.PeerId, Vector2.Zero);
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
            {
                Player.SetUsername(GameManager.Options.OnlineUsername);

                foreach (var pair in GameClient.Players)
                {
                    if (pair.Key == GameClient.PeerId)
                        continue;

                    var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
                    otherPlayer.Position = Vector2.Zero;
                    Players.Add(pair.Key, otherPlayer);
                    AddChild(otherPlayer);
                    otherPlayer.SetUsername(pair.Value);
                }
            }
        }

        public static void UpdatePlayerPositions(Dictionary<uint, Vector2> playerPositions)
        {
            if (SceneManager.ActiveScene != "Game")
                return;

            ServerPlayerPositions = playerPositions;
            
            /*foreach (var pair in playerPositions) 
            {
                var player = Instance.Players[pair.Key];

                if (pair.Key == GameClient.PeerId)
                {
                    //GD.Print("CLIENT: " + player.Position);
                    ServerPlayerPosition = pair.Value;
                    //if (player.Position.DistanceTo(pair.Value) > Test)
                        //player.Position = Utils.Lerp(player.Position, pair.Value, 0.1f);
                }
                else
                    player.Position = pair.Value;
            }*/
        }

        public static int Test = 100;

        public override void _Process(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);

            foreach (var pair in ServerPlayerPositions)
            {
                var player = Players[pair.Key];

                if (pair.Key != GameClient.PeerId)
                    player.Position = Utils.Lerp(player.Position, pair.Value, 0.1f);
            }
        }
    }
}