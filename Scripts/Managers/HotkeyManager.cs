namespace GodotModules
{
    public class HotkeyManager
    {
        public Dictionary<string, HotkeyInfo> DefaultHotkeys;
        public Dictionary<string, HotkeyInfo> PersistentHotkeys;
        public List<string> Categories;

        private SystemFileManager _systemFileManager;

        public HotkeyManager()
        {
            _systemFileManager = new();
            Categories = new();
        }

        public void AddCategories(params string[] v) => Categories.AddRange(v);

        public void Init()
        {
            LoadDefaults();
            PersistentHotkeys = DeepCopy(DefaultHotkeys);

            LoadPersistent();
        }

        public Dictionary<string, HotkeyInfo> DeepCopy(Dictionary<string, HotkeyInfo> data)
        {
            var dict = new Dictionary<string, HotkeyInfo>(data);
            dict.ForEach(x => x.Value.InputEventInfo = new List<InputEventInfo>(data[x.Key].InputEventInfo));
            return dict;
        }

        private void LoadDefaults()
        {
            // make sure all lists are defined
            DefaultHotkeys = new Dictionary<string, HotkeyInfo>();
            foreach (string actionGroup in InputMap.GetActions())
            {
                var actions = InputMap.GetActionList(actionGroup);

                if (actions.Count == 0)
                    continue;

                DefaultHotkeys[actionGroup] = new HotkeyInfo
                {
                    Category = "ui",
                    InputEventInfo = new List<InputEventInfo>()
                };
            }

            foreach (string actionGroup in InputMap.GetActions())
            {
                var actions = InputMap.GetActionList(actionGroup);

                if (actions.Count == 0)
                    continue;

                var category = GetCategory(actionGroup);
                DefaultHotkeys[actionGroup].Category = category;

                foreach (var action in actions)
                {
                    var inputEventInfo = ConvertToInputEventInfo(action);
                    DefaultHotkeys[actionGroup].InputEventInfo.Add(inputEventInfo);
                }
            }
        }

        private void LoadPersistent()
        {
            if (!_systemFileManager.ConfigExists("hotkeys"))
                return;

            var hotkeys = _systemFileManager.ReadConfig<Dictionary<string, HotkeyInfo>>("hotkeys");

            foreach (var pair in hotkeys)
            {
                var action = pair.Key;

                InputMap.ActionEraseEvents(action);
                foreach (var inputEventInfo in pair.Value.InputEventInfo)
                    InputMap.ActionAddEvent(action, inputEventInfo.ConvertToInputEvent());
            }

            PersistentHotkeys = DeepCopy(hotkeys);
        }

        public void ResetHotkey(string action)
        {
            PersistentHotkeys[action] = new HotkeyInfo
            {
                Category = GetCategory(action),
                InputEventInfo = new List<InputEventInfo>()
            };

            foreach (var inputEventInfo in DefaultHotkeys[action].InputEventInfo)
                PersistentHotkeys[action].InputEventInfo.Add(inputEventInfo);
        }

        public void SetHotkey(string action, InputEvent inputEvent)
        {
            PersistentHotkeys[action] = new HotkeyInfo
            {
                Category = GetCategory(action),
                InputEventInfo = new List<InputEventInfo> { ConvertToInputEventInfo(inputEvent) }
            };

            InputMap.ActionEraseEvents(action);
            InputMap.ActionAddEvent(action, inputEvent);
        }

        public void ResetAllHotkeysToDefaults()
        {
            PersistentHotkeys = DeepCopy(DefaultHotkeys);

            foreach (var pair in DefaultHotkeys)
            {
                var action = pair.Key;

                InputMap.ActionEraseEvents(action);
                foreach (var inputEventInfo in pair.Value.InputEventInfo)
                    InputMap.ActionAddEvent(action, inputEventInfo.ConvertToInputEvent());
            }
        }

        public void SaveHotkeys() => _systemFileManager.WriteConfig("hotkeys", PersistentHotkeys);

        public InputEventInfo ConvertToInputEventInfo(object action)
        {
            if (action is InputEvent inputEvent)
            {
                switch (action)
                {
                    case InputEventKey k:
                        return new InputEventInfo
                        {
                            InputEventInfoType = InputEventInfoType.Key,
                            Alt = k.Alt,
                            Command = k.Command,
                            Control = k.Control,
                            Device = k.Device,
                            Echo = k.Echo,
                            Meta = k.Meta,
                            PhysicalScancode = k.PhysicalScancode,
                            Pressed = k.Pressed,
                            Scancode = k.Scancode,
                            Shift = k.Shift,
                            Unicode = k.Unicode
                        };
                    case InputEventMouseButton m:
                        return new InputEventInfo
                        {
                            InputEventInfoType = InputEventInfoType.MouseButton,
                            Alt = m.Alt,
                            Command = m.Command,
                            Control = m.Control,
                            Device = m.Device,
                            Meta = m.Meta,
                            Pressed = m.Pressed,
                            Shift = m.Shift,
                            ButtonIndex = m.ButtonIndex,
                            ButtonMask = m.ButtonMask,
                            DoubleClick = m.Doubleclick,
                            Factor = m.Factor,
                        };
                    case InputEventJoypadButton j:
                        return new InputEventInfo
                        {
                            InputEventInfoType = InputEventInfoType.JoypadButton,
                            Device = j.Device,
                            Pressed = j.Pressed,
                            ButtonIndex = j.ButtonIndex,
                            Pressure = j.Pressure
                        };
                    case InputEventJoypadMotion jm:
                        return new InputEventInfo
                        {
                            InputEventInfoType = InputEventInfoType.JoypadMotion,
                            Device = jm.Device,
                            Axis = jm.Axis,
                            AxisValue = jm.AxisValue
                        };
                }
            }

            throw new InvalidOperationException($"Could not convert {action.GetType()} to InputEventInfo because it is not a supported InputEvent type.");
        }

        public string GetCategory(string actionGroup)
        {
            foreach (var category in Categories)
                if (actionGroup.ToLower().Contains(category.ToLower()))
                    return category.ToLower();

            return Categories[0];
        }
    }

    public record struct InputEventInfo
    {
        public InputEventInfoType InputEventInfoType { get; set; }
        public int Device { get; set; }
        public int ButtonIndex { get; set; }
        public bool Pressed { get; set; }
        public float Pressure { get; set; }
        public bool Alt { get; set; }
        public bool Command { get; set; }
        public bool Control { get; set; }
        public bool Meta { get; set; }
        public bool Shift { get; set; }
        public bool Echo { get; set; }
        public uint PhysicalScancode { get; set; }
        public uint Scancode { get; set; }
        public uint Unicode { get; set; }
        public int ButtonMask { get; set; }
        public bool DoubleClick { get; set; }
        public float Factor { get; set; }
        public int Axis { get; set; }
        public float AxisValue { get; set; }
    }

    public class HotkeyInfo
    {
        public string Category { get; set; }
        public List<InputEventInfo> InputEventInfo { get; set; }
    }

    public enum InputEventInfoType
    {
        Key,
        MouseButton,
        JoypadButton,
        JoypadMotion
    }
}