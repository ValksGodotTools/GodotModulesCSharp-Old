using Godot;
using System;

namespace GodotModules
{
    public class UIControls : Node
    {
        private PackedScene PrefabUIHotkey = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/UIHotkey.tscn");

        public override void _Ready()
        {
            var hotkey = PrefabUIHotkey.Instance<UIHotkey>();
            hotkey.Init("player_move_left");
            AddChild(hotkey);
        }
    }
}
