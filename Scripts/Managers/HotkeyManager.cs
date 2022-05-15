using Godot;

namespace GodotModules
{
    public class HotkeyManager
    {
        private Dictionary<string, Hotkey> _defaultHotkeys;
        private Dictionary<string, Hotkey> _hotkeys;
        public Dictionary<string, Hotkey> Hotkeys => _hotkeys;
        private readonly SystemFileManager _systemFileManager;

        public HotkeyManager(SystemFileManager systemFileManager)
        {
            _hotkeys = new();
            _defaultHotkeys = new();
            _systemFileManager = systemFileManager;
            LoadDefaultHotkeys();
            if (_systemFileManager.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public void LoadPersistentHotkeys()
        {
            var jsonData = _systemFileManager.ReadConfig<Dictionary<string, JsonHotkey>>("controls");
            _hotkeys = jsonData.ToDictionary(x => x.Key, x => ConvertJsonToHotkey(x.Value));

            foreach (var pair in _hotkeys)
                SetHotkeyEvent(pair.Key, pair.Value);
        }

        public void ResetHotkey(string key)
        {
            _hotkeys[key] = _defaultHotkeys[key];
            SetHotkeyEvent(key, _hotkeys[key]);
        }

        public void ResetToDefaultHotkeys()
        {
            _hotkeys = new(_defaultHotkeys);

            foreach (var pair in _hotkeys)
                SetHotkeyEvent(pair.Key, pair.Value);
        }

        private void LoadDefaultHotkeys()
        {
            foreach (string action in InputMap.GetActions())
            {
                var arr = InputMap.GetActionList(action);

                if (arr.Count == 0)
                    continue;

                var category = GetHotkeyCategory(action);

                _defaultHotkeys[action] = new Hotkey(category, (InputEventKey)InputMap.GetActionList(action)[0]);
            }

            _hotkeys = new(_defaultHotkeys);
        }

        private HotkeyCategory GetHotkeyCategory(string action)
        {
            var text = action.ToLower();

            var hotkeyCategory = HotkeyCategory.UI;

            if (text.Contains("ui"))
                hotkeyCategory = HotkeyCategory.UI;
            if (text.Contains("player"))
                hotkeyCategory = HotkeyCategory.Player;

            return hotkeyCategory;
        }

        public void SaveHotkeys() => _systemFileManager.WriteConfig("controls", _hotkeys.ToDictionary(x => x.Key, x => ConvertHotkeyToJson(x.Value)));
        public void SetHotkey(string action, InputEventKey inputEventKey)
        {
            var hotkey = new Hotkey(GetHotkeyCategory(action), inputEventKey);
            SetHotkeyEvent(action, hotkey);
            _hotkeys[action] = hotkey;
        }

        private void SetHotkeyEvent(string action, Hotkey hotkey)
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, hotkey.InputEventKey);
        }

        private JsonHotkey ConvertHotkeyToJson(Hotkey hotkey) 
        {
            var inputEventKey = hotkey.InputEventKey;
            return new JsonHotkey
            {
                Category = hotkey.Category,
                Scancode = inputEventKey.Scancode,
                PhysicalScancode = inputEventKey.PhysicalScancode,
                Unicode = inputEventKey.Unicode,
                Alt = inputEventKey.Alt,
                Shift = inputEventKey.Shift,
                Control = inputEventKey.Control,
                Meta = inputEventKey.Meta,
                Command = inputEventKey.Command,
                Device = inputEventKey.Device
            };
        }
            

        private Hotkey ConvertJsonToHotkey(JsonHotkey jsonHotkey) =>
            new Hotkey(jsonHotkey.Category, new InputEventKey()
            {
                Scancode = jsonHotkey.Scancode,
                PhysicalScancode = jsonHotkey.PhysicalScancode,
                Unicode = jsonHotkey.Unicode,
                Alt = jsonHotkey.Alt,
                Shift = jsonHotkey.Shift,
                Control = jsonHotkey.Control,
                Meta = jsonHotkey.Meta,
                Command = jsonHotkey.Command,
                Device = jsonHotkey.Device
            });
    }

    public class Hotkey
    {
        public HotkeyCategory Category { get; set; }
        public InputEventKey InputEventKey { get; set; }

        public Hotkey(HotkeyCategory category, InputEventKey inputEventKey)
        {
            Category = category;
            InputEventKey = inputEventKey;
        }
    }

    public enum HotkeyCategory
    {
        UI,
        Player
    }

    public struct JsonHotkey
    {
        public HotkeyCategory Category { get; set; }
        public uint Scancode { get; set; }
        public uint PhysicalScancode { get; set; }
        public uint Unicode { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Control { get; set; }
        public bool Meta { get; set; }
        public bool Command { get; set; }
        public int Device { get; set; }
    }
}