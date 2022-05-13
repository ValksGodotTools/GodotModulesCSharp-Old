using Godot;
using System;

namespace GodotModules
{
    public class UIControls : Control
    {
        [Export] public readonly NodePath NodePathVBoxHotkeys;
        private Control _vboxHotkeys;

        private HotkeyManager _hotkeyManager { get; set; }
        private Dictionary<string, UIHotkey> _uiHotkeys = new Dictionary<string, UIHotkey>();

        public void PreInit(HotkeyManager hotkeyManager)
        {
            _hotkeyManager = hotkeyManager;
        }

        public override void _Ready()
        {
            _vboxHotkeys = GetNode<Control>(NodePathVBoxHotkeys);
            foreach (var action in _hotkeyManager.Hotkeys.Keys.OrderBy(x => x).ToList()) 
            {
                var hotkeyInstance = Prefabs.UIHotkey.Instance<UIHotkey>();
                hotkeyInstance.Init(_hotkeyManager, action);
                _vboxHotkeys.AddChild(hotkeyInstance);
                _uiHotkeys[action] = hotkeyInstance;
            }
        }

        private void _on_Reset_Hotkeys_pressed()
        {
            _hotkeyManager.ResetToDefaultHotkeys();

            foreach (var pair in _hotkeyManager.Hotkeys) 
                _uiHotkeys[pair.Key].SetHotkeyText(pair.Value.AsText());
        }
    }
}
