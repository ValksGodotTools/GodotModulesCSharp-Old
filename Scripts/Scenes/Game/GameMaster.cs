using Godot;
using GodotModules.ModLoader;
using System;

namespace Game
{
    // Master manages everything in the game (DEMO)
    public class GameMaster : Node
    {
        public static Player Player { get; set; }
        private static GameMaster Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public static Label LabelPlayerHealth;

        public override void _Ready()
        {
            Instance = this;
            InitNodes();
            InitPlayer();

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)GameMaster.Player.SetHealth;

            ModLoader.Call("OnGameInit");
        }

        public override void _Process(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);
        }

        private static void InitNodes()
        {
            LabelPlayerHealth = Instance.GetNode<Label>(Instance.NodePathLabelPlayerHealth);
        }

        private static void InitPlayer()
        {
            var playerPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Player.tscn");
            Player = playerPrefab.Instance<Player>();
            Player.Position = OS.WindowSize / 2;
            Player.Name = "Player";
            Instance.AddChild(Player);
        }
    }
}