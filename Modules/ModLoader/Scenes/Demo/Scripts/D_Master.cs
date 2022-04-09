using Godot;
using System;
using Valk.ModLoader;

namespace D_Game
{
    // Master manages everything in the game (DEMO)
    public class D_Master : Node
    {
        public static D_Player Player { get; set; }
        private static D_Master Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public static Label LabelPlayerHealth;

        public override void _Ready()
        {
            Instance = this;
            InitNodes();
            InitPlayer();

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)D_Master.Player.SetHealth;
            
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
            var playerPrefab = ResourceLoader.Load<PackedScene>("res://Modules/ModLoader/Scenes/Prefabs/Player.tscn");
            Player = playerPrefab.Instance<D_Player>();
            Player.Position = OS.WindowSize / 2;
            Player.Name = "Player";
            Instance.AddChild(Player);
        }
    }
}
