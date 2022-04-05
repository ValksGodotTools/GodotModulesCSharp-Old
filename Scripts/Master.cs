using Godot;
using System;
using System.IO;

using Path = System.IO.Path;
using Directory = System.IO.Directory;

namespace ModLoader
{
    public class Master : Node
    {
        public static Player Player { get; set; }
        private static Master Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public static Label LabelPlayerHealth;

        public override void _Ready()
        {
            Instance = this;
            InitNodes();
            InitPlayer();
            ModLoader.LoadGameDefs();
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
