using Godot;

namespace GodotModules
{
    public struct HotkeyInfo
    {
        public string Action { get; }
        public string Category { get; }
        public InputEvent Event { get; }

        public HotkeyInfo(string action, string category, InputEvent e)
        {
            Action = action;
            Category = category;
            Event = e;
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

            foreach (var e in data)
                _hotkeys[e.Action] = new(e.Action, e.Category, ConvertToInput(e));

            foreach (var hotkey in Hotkeys)
                SetHotkeyEvent(hotkey.Action, hotkey.Event);
        }

        public void ResetHotkey(string key)
        {
            _hotkeys[key] = _defaultHotkeys[key];
            SetHotkeyEvent(key, _hotkeys[key].Event);
        }

        public void ResetToDefaultHotkeys()
        {
            _hotkeys = new(_defaultHotkeys);

            foreach (var hotkey in Hotkeys)
                SetHotkeyEvent(hotkey.Action, hotkey.Event);
        }

        private void LoadDefaultHotkeys()
        {
            foreach (string action in InputMap.GetActions())
            {
                var actionList = InputMap.GetActionList(action);

                if (actionList.Count == 0)
                    continue;

                if
                (
                    actionList[0] is InputEvent e && 
                    (
                        e is InputEventKey || 
                        e is InputEventMouseButton || 
                        e is InputEventJoypadButton
                    )
                ) {
                    _defaultHotkeys[action] = new(action, GetHotkeyCategory(action), e);
                }
            }

            _hotkeys = new(_defaultHotkeys);
        }

        public void SaveHotkeys()
        {
            _systemFileManager.WriteConfig("controls", _hotkeys.Values.Select(ConvertToJson).ToList());
        }

        public void SetHotkey(string action, InputEvent e)
        {
            _hotkeys[action] = new HotkeyInfo(action, GetHotkeyCategory(action), e);
            SetHotkeyEvent(action, e);
        }

        private void SetHotkeyEvent(string action, InputEvent e)
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, e);
        }

        public string GetHotkeyCategory(string action)
        {
            var text = action.ToLower();

            foreach (var category in _categories)
                if (text.Contains(category.ToLower()))
                    return category;

            return _categories[0];
        }

        private JsonInputKey ConvertToJson(HotkeyInfo info) => new JsonInputKey
        {
            Action = info.Action,
            Category = info.Category,
            Info = InputEventInfo.TryFrom(info.Event) ?? throw new Exception("Invalid input event"),
        };

        private InputEvent ConvertToInput(JsonInputKey inputEvent) => 
            inputEvent.Info.ToEvent();
    }

    public struct JsonInputKey
    {
        public string Action { get; set; }
        public string Category { get; set; }
        public InputEventInfo Info { get; set; }
    }
}