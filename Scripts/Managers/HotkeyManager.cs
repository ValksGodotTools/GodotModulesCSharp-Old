using Godot;

namespace GodotModules
{
    public class HotkeyManager
    {
        private Dictionary<HotkeyCategory, Dictionary<string, InputEventKey>> _defaultHotkeys;
        private Dictionary<HotkeyCategory, Dictionary<string, InputEventKey>> _hotkeys;
        public Dictionary<HotkeyCategory, Dictionary<string, InputEventKey>> Hotkeys => _hotkeys;
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
            var jsonData = _systemFileManager.ReadConfig<Dictionary<HotkeyCategory, List<JsonInputKey>>>("controls");

            var dict = new Dictionary<HotkeyCategory, Dictionary<string, InputEventKey>>();

            foreach (var category in Utils.GetEnumList<HotkeyCategory>())
                dict.Add(category, new());

            foreach (var pair1 in jsonData)
                foreach (var pair2 in pair1.Value)
                    dict[pair1.Key][pair2.Action] = ConvertToInputKey(pair2);

            _hotkeys = dict;

            foreach (var pair1 in _hotkeys)
                foreach (var pair2 in pair1.Value) 
                    SetHotkeyEvent(pair2.Key, pair2.Value);
        }

        public void ResetHotkey(string key) 
        {
            var category = GetHotkeyCategory(key);
            _hotkeys[category][key] = _defaultHotkeys[category][key];
            SetHotkeyEvent(key, _hotkeys[category][key]);
        }

        public void ResetToDefaultHotkeys()
        {
            _hotkeys = new(_defaultHotkeys);

            foreach (var pair1 in _hotkeys)
                foreach (var pair2 in pair1.Value)
                    SetHotkeyEvent(pair2.Key, pair2.Value);
        }

        private void LoadDefaultHotkeys()
        {
            foreach (var category in Utils.GetEnumList<HotkeyCategory>())
                _defaultHotkeys.Add(category, new());

            foreach (string action in InputMap.GetActions())
            {
                var actionList = InputMap.GetActionList(action);

                if (actionList.Count == 0)
                    continue;

                var category = GetHotkeyCategory(action);

                _defaultHotkeys[category][action] = (InputEventKey)actionList[0];
            }

            _hotkeys = new(_defaultHotkeys);
        }

        public void SaveHotkeys() 
        {
            var json = new Dictionary<HotkeyCategory, List<JsonInputKey>>();

            foreach (var category in Utils.GetEnumList<HotkeyCategory>())
                json.Add(category, new());

            foreach (var pair1 in _hotkeys)
                foreach (var pair2 in pair1.Value)
                    json[pair1.Key].Add(ConvertToJson(pair2.Key, pair2.Value));

            _systemFileManager.WriteConfig("controls", json);
        }

        public void SetHotkey(string action, InputEventKey inputEventKey) 
        {
            SetHotkeyEvent(action, inputEventKey);
            var category = GetHotkeyCategory(action);
            _hotkeys[category][action] = inputEventKey;
        }

        private void SetHotkeyEvent(string action, InputEventKey inputEventKey) 
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEventKey);
        }

        public HotkeyCategory GetHotkeyCategory(string action)
        {
            var text = action.ToLower();

            var hotkeyCategory = HotkeyCategory.UI;

            if (text.Contains("ui"))
                hotkeyCategory = HotkeyCategory.UI;
            if (text.Contains("player"))
                hotkeyCategory = HotkeyCategory.Player;

            return hotkeyCategory;
        }

        private JsonInputKey ConvertToJson(string action, InputEventKey inputEventKey) => 
            new JsonInputKey
            {
                Action = action,
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

        private InputEventKey ConvertToInputKey(JsonInputKey inputEvent) =>
            new InputEventKey()
            {
                Scancode = inputEvent.Scancode,
                PhysicalScancode = inputEvent.PhysicalScancode,
                Unicode = inputEvent.Unicode,
                Alt = inputEvent.Alt,
                Shift = inputEvent.Shift,
                Control = inputEvent.Control,
                Meta = inputEvent.Meta,
                Command = inputEvent.Command,
                Device = inputEvent.Device
            };
    }

    public enum HotkeyCategory 
    {
        UI,
        Player
    }

    public struct JsonInputKey
    {
        public string Action { get; set; }
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