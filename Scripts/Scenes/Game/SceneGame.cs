using Godot;
using GodotModules;
using GodotModules.ModLoader;
using GodotModules.Netcode.Client;
using System;

namespace Game
{
    public class SceneGame : Node
    {
        public static ClientPlayer Player { get; set; }
        public static SceneGame Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public Label LabelPlayerHealth;


        public override void _Ready()
        {
            Instance = this;
            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            Player = Prefabs.ClientPlayer.Instance<ClientPlayer>();
            Player.Position = OS.WindowSize / 2;
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
                    otherPlayer.Position = OS.WindowSize / 2;
                    AddChild(otherPlayer);
                    otherPlayer.SetUsername(pair.Value);
                }
            }
        }

        public override void _Process(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);
        }
    }
}