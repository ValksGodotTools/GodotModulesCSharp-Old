using Godot;

namespace GodotModules
{
    public class UIOptionsControls : Control
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
            foreach (var pair1 in _hotkeyManager.Hotkeys)
            {
                foreach (var pair2 in pair1.Value)
                {
                    var hotkeyInstance = Prefabs.UIHotkey.Instance<UIHotkey>();
                    hotkeyInstance.Init(_hotkeyManager, pair2.Key);
                    _vboxHotkeys.AddChild(hotkeyInstance);
                    _uiHotkeys[pair2.Key] = hotkeyInstance;
                }
            }
        }

        private void _on_Reset_Hotkeys_pressed()
        {
            _hotkeyManager.ResetToDefaultHotkeys();

            foreach (var pair1 in _hotkeyManager.Hotkeys)
                foreach (var pair2 in pair1.Value)
                    _uiHotkeys[pair2.Key].SetHotkeyText(pair2.Value.AsText()); 
        }
    }
}
