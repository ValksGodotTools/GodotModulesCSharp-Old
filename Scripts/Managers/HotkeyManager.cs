using Godot;

using HotkeyMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Godot.InputEventKey>>;

namespace GodotModules
{
    public struct HotkeyInfo
    {
        public string Action { get; }
        public string Category { get; }
        public InputEventKey Key { get; }

        public HotkeyInfo(string action, string category, InputEventKey key)
        {
            Action = action;
            Category = category;
            Key = key;
        }
    }

    public class HotkeyManager
    {
        // Data is stored like this -> Dictionary<Category, Dictioanry<Action, InputEventKey>>
        private Dictionary<string, HotkeyInfo> _defaultHotkeys = new();
        private Dictionary<string, HotkeyInfo> _hotkeys = new();
        public IEnumerable<HotkeyInfo> Hotkeys => _hotkeys.Values;
        public IDictionary<String, HotkeyInfo> HotkeysByAction => _hotkeys;
        private readonly SystemFileManager _systemFileManager;
        private List<string> _categories;
        public IEnumerable<string> Categories => _categories;

        public HotkeyManager(SystemFileManager systemFileManager, List<string> categories)
        {
            _systemFileManager = systemFileManager;
            _categories = categories;

            LoadDefaultHotkeys();

            if (_systemFileManager.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public IEnumerable<HotkeyInfo> GetHotkeysForCategory(string category) =>
            _hotkeys.Where(x => x.Value.Category == category).Select(x => x.Value);

        public IEnumerable<string> GetUsedCategories() =>
            _hotkeys.Select(x => x.Value.Category).Distinct();

        public void LoadPersistentHotkeys()
        {
            var data = _systemFileManager.ReadConfig<List<JsonInputKey>>("controls");
            _hotkeys = new();

            foreach (var key in data)
                _hotkeys[key.Action] = new(key.Action, key.Category, ConvertToInputKey(key));

            foreach (var hotkey in _hotkeys)
                SetHotkeyEvent(hotkey.Key, hotkey.Value.Key);
        }

        public void ResetHotkey(string key)
        {
            _hotkeys[key] = _defaultHotkeys[key];
            SetHotkeyEvent(key, _hotkeys[key].Key);
        }

        public void ResetToDefaultHotkeys()
        {
            _hotkeys = new(_defaultHotkeys);

            foreach (var hotkey in _hotkeys)
                SetHotkeyEvent(hotkey.Key, hotkey.Value.Key);
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
                    _defaultHotkeys[action] = new(action, GetHotkeyCategory(action), inputEventKey);

                if (t is InputEventMouseButton inputEventMouseButton)
                    Logger.LogTodo("Mouse event not implemented yet");
            }

            _hotkeys = new(_defaultHotkeys);
        }

        public void SaveHotkeys()
        {
            _systemFileManager.WriteConfig("controls", _hotkeys.Values.Select(ConvertToJson).ToList());
        }

        public void SetHotkey(string action, InputEventKey inputEventKey)
        {
            _hotkeys[action] = new HotkeyInfo(action, GetHotkeyCategory(action), inputEventKey);
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

        private JsonInputKey ConvertToJson(HotkeyInfo info) => new JsonInputKey
        {
            Action = info.Action,
            Category = info.Category,
            Scancode = info.Key.Scancode,
            PhysicalScancode = info.Key.PhysicalScancode,
            Unicode = info.Key.Unicode,
            Alt = info.Key.Alt,
            Shift = info.Key.Shift,
            Control = info.Key.Control,
            Meta = info.Key.Meta,
            Command = info.Key.Command,
            Device = info.Key.Device
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
        public string Category { get; set;}
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