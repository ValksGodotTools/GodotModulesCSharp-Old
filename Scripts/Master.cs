using Godot;
using System;

namespace Game
{
    public class Master : Node
    {
        public static Player Player { get; set; }
        private static Master Instance { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        private Label LabelPlayerHealth;

        public override void _Ready()
        {
            Instance = this;
            CreatePlayer();

            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            LabelPlayerHealth.Text = $"Health: {Player.Health}";
        }

        private static void CreatePlayer()
        {
            var playerPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/Player.tscn");
            Player = playerPrefab.Instance<Player>();
            Player.Position = OS.WindowSize / 2;
            Instance.AddChild(Player);
        }
    }

}
