using Godot;

namespace GodotModules
{
    public class UIOptionsControls : Control
    {
        [Export] public readonly NodePath NodePathTabContainer;
        private TabContainer _tabContainer;

        private HotkeyManager _hotkeyManager { get; set; }
        private Dictionary<string, UIHotkey> _uiHotkeys = new Dictionary<string, UIHotkey>();

        public void PreInit(HotkeyManager hotkeyManager)
        {
            _hotkeyManager = hotkeyManager;
        }

        public override void _Ready()
        {
            _tabContainer = GetNode<TabContainer>(NodePathTabContainer);

            var vboxs = new Dictionary<string, VBoxContainer>();

            foreach (var category in _hotkeyManager.Categories)
            {
                var panelContainer = new PanelContainer();
                panelContainer.Name = "" + category;

                var scrollContainer = new ScrollContainer();
                panelContainer.AddChild(scrollContainer);

                var vboxContainer = new VBoxContainer();
                vboxContainer.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                vboxContainer.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                scrollContainer.AddChild(vboxContainer);

                vboxs.Add(category, vboxContainer);
                _tabContainer.AddChild(panelContainer);
            }

            foreach (var hotkey in _hotkeyManager.Hotkeys)
            {
                var hotkeyInstance = Prefabs.UIHotkey.Instance<UIHotkey>();
                hotkeyInstance.Init(_hotkeyManager, hotkey.Action);
                vboxs[hotkey.Category].AddChild(hotkeyInstance);
                _uiHotkeys[hotkey.Action] = hotkeyInstance;
            }
        }

        private void _on_Reset_Hotkeys_pressed()
        {
            _hotkeyManager.ResetToDefaultHotkeys();

            foreach (var hotkey in _hotkeyManager.Hotkeys)
                _uiHotkeys[hotkey.Action].SetHotkeyText(hotkey.Key.AsText()); 
        }
    }
}
