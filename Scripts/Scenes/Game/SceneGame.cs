using Godot;
using GodotModules;
using GodotModules.ModLoader;
using System;

namespace Game
{
    // Master manages everything in the game (DEMO)
    public class SceneGame : Node
    {
        private static PackedScene PrefabPlayer = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Player.tscn");
        public static Player Player { get; set; }
        public static SceneGame Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public Label LabelPlayerHealth;


        public override void _Ready()
        {
            Instance = this;
            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            InitPlayer();

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)SceneGame.Player.SetHealth;

            ModLoader.Call("OnGameInit");
        }

        public override void _Process(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);
        }

        private void InitPlayer()
        {
            Player = PrefabPlayer.Instance<Player>();
            Player.Position = OS.WindowSize / 2;
            Player.Name = "Player";
            AddChild(Player);
        }
    }
}