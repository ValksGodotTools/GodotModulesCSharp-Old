using Godot;

namespace GodotModules
{
    public class HotkeyManager
    {
        private Dictionary<string, JsonInputKey> _hotkeys = new Dictionary<string, JsonInputKey>();
        private readonly SystemFileManager _systemFileManager;

        public HotkeyManager(SystemFileManager systemFileManager)
        {
            _systemFileManager = systemFileManager;
            LoadDefaultHotkeys();
            if (_systemFileManager.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public void LoadPersistentHotkeys()
        {
            _hotkeys = _systemFileManager.ReadConfig<Dictionary<string, JsonInputKey>>("controls");

            foreach (var pair in _hotkeys)
            {
                var inputEvent = pair.Value;
                var inputKey = ConvertJsonInputKeyToInputKey(inputEvent);

                InputMap.ActionEraseEvents(pair.Key);
                InputMap.ActionAddEvent(pair.Key, inputKey);
            }
        }

        private void LoadDefaultHotkeys()
        {
            foreach (string action in InputMap.GetActions())
            {
                var arr = InputMap.GetActionList(action);

                if (arr.Count == 0)
                    continue;

                var inputEvent = (InputEventKey)InputMap.GetActionList(action)[0];
                var inputKey = ConvertInputKeyToJsonInputKey(inputEvent);

                _hotkeys[action] = inputKey;
            }
        }

        public void SaveHotkeys() => _systemFileManager.WriteConfig("controls", _hotkeys);
        public void SetHotkey(string action, InputEventKey inputEventKey) 
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEventKey);
            _hotkeys[action] = ConvertInputKeyToJsonInputKey(inputEventKey);
        }

        private JsonInputKey ConvertInputKeyToJsonInputKey(InputEventKey inputEventKey) => 
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

        private InputEventKey ConvertJsonInputKeyToInputKey(JsonInputKey inputEvent) =>
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