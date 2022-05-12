using Godot;

namespace GodotModules
{
    public class OptionsManager
    {
        public Dictionary<string, JsonInputKey> Hotkeys = new Dictionary<string, JsonInputKey>();

        public OptionsManager()
        {
            LoadDefaultHotkeys();
            if (GM.ConfigExists("controls"))
                LoadPersistentHotkeys();
        }

        public void LoadPersistentHotkeys()
        {
            Hotkeys = GM.ReadConfig<Dictionary<string, JsonInputKey>>("controls");

            foreach (var pair in Hotkeys)
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

                Hotkeys[action] = inputKey;
            }
        }

        public void SaveHotkeys() => GM.WriteConfig("controls", Hotkeys);
        public void SetHotkey(string action, InputEventKey inputEventKey) 
        {
            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEventKey);
            Hotkeys[action] = ConvertInputKeyToJsonInputKey(inputEventKey);
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