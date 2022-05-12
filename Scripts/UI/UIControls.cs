using Godot;
using System;

namespace GodotModules
{
    public class UIControls : Node
    {
        public HotkeyManager _hotkeyManager { get; set; }

        public override void _Ready()
        {
            var hotkey = Prefabs.UIHotkey.Instance<UIHotkey>();
            hotkey.Init(_hotkeyManager, "player_move_left");
            AddChild(hotkey);
        }
    }
}
