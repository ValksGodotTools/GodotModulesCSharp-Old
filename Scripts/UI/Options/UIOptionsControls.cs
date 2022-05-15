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

            var vboxs = new Dictionary<HotkeyCategory, VBoxContainer>();

            foreach (var pair1 in _hotkeyManager.Hotkeys)
            {
                var panelContainer = new PanelContainer();
                panelContainer.Name = "" + pair1.Key;

                var scrollContainer = new ScrollContainer();
                panelContainer.AddChild(scrollContainer);

                var vboxContainer = new VBoxContainer();
                vboxContainer.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                vboxContainer.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                scrollContainer.AddChild(vboxContainer);

                vboxs[pair1.Key] = vboxContainer;
                _tabContainer.AddChild(panelContainer);
            }

            foreach (var pair1 in _hotkeyManager.Hotkeys)
            {
                foreach (var pair2 in pair1.Value)
                {
                    var hotkeyInstance = Prefabs.UIHotkey.Instance<UIHotkey>();
                    hotkeyInstance.Init(_hotkeyManager, pair2.Key);
                    vboxs[pair1.Key].AddChild(hotkeyInstance);
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
