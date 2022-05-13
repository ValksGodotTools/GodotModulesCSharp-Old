using Godot;
using System;

namespace GodotModules
{
    public class UIControls : Control
    {
        public HotkeyManager _hotkeyManager { get; set; }

        public override void _Ready()
        {
            foreach (var action in _hotkeyManager.Hotkeys.Keys.OrderBy(x => x).ToList()) 
            {
                var hotkeyInstance = Prefabs.UIHotkey.Instance<UIHotkey>();
                hotkeyInstance.Init(_hotkeyManager, action);
                AddChild(hotkeyInstance);
            }
        }
    }
}
