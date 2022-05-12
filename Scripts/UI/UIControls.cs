using Godot;
using System;

namespace GodotModules
{
    public class UIControls : Node
    {
        public override void _Ready()
        {
            var hotkey = Prefabs.UIHotkey.Instance<UIHotkey>();
            hotkey.Init("player_move_left");
            AddChild(hotkey);
        }
    }
}
