using Godot;
using System;

namespace GodotModules
{
    public class UIHotkey : Node
    {
        [Export] public readonly NodePath NodePathLabel;
        [Export] public readonly NodePath NodePathBtnHotkey;

        private Label _label;
        private UIBtnHotkey _btnHotkey;

        private HotkeyManager _hotkeyManager;
        private string _action;

        public override void _Ready()
        {
            _label = GetNode<Label>(NodePathLabel);
            _btnHotkey = GetNode<UIBtnHotkey>(NodePathBtnHotkey);
            _label.Text = _action.Replace("_", " ").ToTitleCase().SmallWordsToUpper(2, (word) => word != "Up");
            _btnHotkey.PreInit(_hotkeyManager, _action);
        }

        public void Init(HotkeyManager hotkeyManager, string action)
        {
            _hotkeyManager = hotkeyManager;
            _action = action;
        }

        public void SetHotkeyText(string v) => _btnHotkey.SetHotkeyText(v);

        private void _on_Reset_To_Default_pressed() 
        {
            _hotkeyManager.ResetHotkey(_action);
            var category = _hotkeyManager.GetHotkeyCategory(_action);
            Logger.LogDebug(_hotkeyManager.Hotkeys[category][_action].AsText());
            SetHotkeyText(_hotkeyManager.Hotkeys[category][_action].AsText());
        }
    }
}
