using Godot;

namespace GodotModules
{
    public class UIOptionsControls : Control
    {
        [Export] protected readonly NodePath NodePathTabContainer;
        private TabContainer _tabContainer;

        private HotkeyManager _hotkeyManager;
        private Dictionary<string, VBoxContainer> _vboxs;

        public void PreInit(HotkeyManager hotkeyManager)
        {
            _hotkeyManager = hotkeyManager;
        }

        public override void _Ready()
        {
            _tabContainer = GetNode<TabContainer>(NodePathTabContainer);

            _vboxs = new Dictionary<string, VBoxContainer>();

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

                _vboxs.Add(category, vboxContainer);
                _tabContainer.AddChild(panelContainer);
            }

            foreach (var category in _hotkeyManager.Categories)
            {
                var inputEventsCategory = _hotkeyManager.PersistentHotkeys.Where(x => x.Value.Category == category).OrderBy(x => x.Key);

                foreach (var pair in inputEventsCategory)
                {
                    var uiHotkey = Prefabs.UIHotkey.Instance<UIHotkey>();
                    uiHotkey.Init(_hotkeyManager, pair.Key, pair.Value.InputEventInfo[0].Display());
                    _vboxs[category].AddChild(uiHotkey);
                }
            }
        }

        private void _on_Reset_Hotkeys_pressed()
        {
            _hotkeyManager.ResetAllHotkeysToDefaults();

            foreach (var vbox in _vboxs.Values)
                foreach (Node child in vbox.GetChildren())
                    child.QueueFree();

            foreach (var category in _hotkeyManager.Categories)
            {
                var inputEventsCategory = _hotkeyManager.DefaultHotkeys.Where(x => x.Value.Category == category).OrderBy(x => x.Key);

                foreach (var pair in inputEventsCategory)
                {
                    var uiHotkey = Prefabs.UIHotkey.Instance<UIHotkey>();
                    uiHotkey.Init(_hotkeyManager, pair.Key, pair.Value.InputEventInfo[0].Display());
                    _vboxs[category].AddChild(uiHotkey);
                }
            }
        }
    }
}
