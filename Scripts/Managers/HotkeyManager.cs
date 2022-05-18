using Godot;

using HotkeyMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Godot.InputEventKey>>;

namespace GodotModules
{
    public class HotkeyManager
    {
        // Data is stored like this -> Dictionary<Category, Dictioanry<Action, InputEventKey>>
        private HotkeyMap _defaultHotkeys = new();
        private HotkeyMap _hotkeys = new();
        public HotkeyMap Hotkeys => _hotkeys;
        private readonly SystemFileManager _systemFileManager;
        private List<string> _categories;

        public HotkeyManager(SystemFileManager systemFileManager, List<string> categories)
        {
            _systemFileManager = systemFileManager;
            _categories = categories;

            LoadDefaultHotkeys();

            if (_systemFileManager.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public void LoadPersistentHotkeys()
        {
            var jsonData = _systemFileManager.ReadConfig<Dictionary<string, List<JsonInputKey>>>("controls");
            _hotkeys = new HotkeyMap();

            foreach (var category in _categories)
                _hotkeys.Add(category, new());

            foreach (var pair1 in jsonData)
                foreach (var pair2 in pair1.Value)
                    _hotkeys[pair1.Key][pair2.Action] = ConvertToInputKey(pair2);

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
            _hotkeys = Clone(_defaultHotkeys);

            foreach (var pair1 in _defaultHotkeys)
                foreach (var pair2 in pair1.Value)
                    SetHotkeyEvent(pair2.Key, pair2.Value);
        }

        private void LoadDefaultHotkeys()
        {
            foreach (var category in _categories)
                _defaultHotkeys.Add(category, new());

            foreach (string action in InputMap.GetActions())
            {
                var actionList = InputMap.GetActionList(action);

                if (actionList.Count == 0)
                    continue;

                var t = actionList[0];

                if (t is InputEventKey inputEventKey)
                    _defaultHotkeys[GetHotkeyCategory(action)][action] = inputEventKey;

                if (t is InputEventMouseButton inputEventMouseButton)
                    Logger.LogTodo("Mouse event not implemented yet");
            }

            _hotkeys = Clone(_defaultHotkeys);
        }

        public void SaveHotkeys()
        {
            var json = new Dictionary<string, List<JsonInputKey>>();

            foreach (var category in _categories)
                json.Add(category, new());

            foreach (var pair1 in _hotkeys)
                foreach (var pair2 in pair1.Value)
                    json[pair1.Key].Add(ConvertToJson(pair2.Key, pair2.Value));

            _systemFileManager.WriteConfig("controls", json);
        }

        public void SetHotkey(string action, InputEventKey inputEventKey)
        {
            _hotkeys[GetHotkeyCategory(action)][action] = inputEventKey;
            SetHotkeyEvent(action, inputEventKey);
        }

        private void SetHotkeyEvent(string action, InputEventKey inputEventKey)
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEventKey);
        }

        public string GetHotkeyCategory(string action)
        {
            var text = action.ToLower();

            foreach (var category in _categories)
                if (text.Contains(category.ToLower()))
                    return category;

            return _categories[0];
        }

        private HotkeyMap Clone(HotkeyMap dict)
        {
            HotkeyMap clone = new();

            foreach (var pair in dict)
                clone.Add(pair.Key, new(pair.Value));

            return clone;
        }

        private JsonInputKey ConvertToJson(string action, InputEventKey inputEventKey) => new JsonInputKey
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

        private InputEventKey ConvertToInputKey(JsonInputKey inputEvent) => new InputEventKey()
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