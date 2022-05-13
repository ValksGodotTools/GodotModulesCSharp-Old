using Godot;

namespace GodotModules
{
    public class HotkeyManager
    {
        private Dictionary<string, InputEventKey> _defaultHotkeys;
        private Dictionary<string, InputEventKey> _hotkeys;
        public Dictionary<string, InputEventKey> Hotkeys => _hotkeys;
        private readonly SystemFileManager _systemFileManager;

        public HotkeyManager(SystemFileManager systemFileManager)
        {
            _hotkeys = new();
            _systemFileManager = systemFileManager;
            LoadDefaultHotkeys();
            if (_systemFileManager.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public void LoadPersistentHotkeys()
        {
            var jsonData = _systemFileManager.ReadConfig<Dictionary<string, JsonInputKey>>("controls");
            _hotkeys = jsonData.ToDictionary(x => x.Key, x => ConvertToInputKey(x.Value));

            foreach (var pair in _hotkeys)
            {
                InputMap.ActionEraseEvents(pair.Key);
                InputMap.ActionAddEvent(pair.Key, pair.Value);
            }
        }

        public void ResetToDefaultHotkeys()
        {
            _hotkeys = _defaultHotkeys;

            foreach (var pair in _hotkeys) 
            {
                InputMap.ActionEraseEvents(pair.Key);
                InputMap.ActionAddEvent(pair.Key, pair.Value);
            }
        }

        private void LoadDefaultHotkeys()
        {
            foreach (string action in InputMap.GetActions())
            {
                var arr = InputMap.GetActionList(action);

                if (arr.Count == 0)
                    continue;

                _hotkeys[action] = (InputEventKey)InputMap.GetActionList(action)[0];
            }

            _defaultHotkeys = _hotkeys;
        }

        public void SaveHotkeys() => _systemFileManager.WriteConfig("controls", _hotkeys.ToDictionary(x => x.Key, x => ConvertToJson(x.Value)));
        public void SetHotkey(string action, InputEventKey inputEventKey) 
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEventKey);
            _hotkeys[action] = inputEventKey;
        }

        private JsonInputKey ConvertToJson(InputEventKey inputEventKey) => 
            new JsonInputKey
            {
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

    public struct JsonInputKey
    {
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