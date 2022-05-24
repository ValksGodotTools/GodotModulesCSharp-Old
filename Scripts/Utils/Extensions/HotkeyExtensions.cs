

namespace GodotModules 
{
    public static class HotkeyExtensions 
    {
        private static readonly Dictionary<int, string> _mouseButtons = new Dictionary<int, string>
        {
            { (int)ButtonList.Left, "Left Click" },
            { (int)ButtonList.Right, "Right Click" },
            { (int)ButtonList.Middle, "Middle Click" },
            { (int)ButtonList.Xbutton1, "Mouse XBtn 1" },
            { (int)ButtonList.Xbutton2, "Mouse XBtn 2" },
            { (int)ButtonList.WheelUp, "Wheel Up" },
            { (int)ButtonList.WheelDown, "Wheel Down" },
            { (int)ButtonList.WheelLeft, "Wheel Left" },
            { (int)ButtonList.WheelRight, "Wheel Right" },
        };

        public static string Display(this InputEventInfo e)
        {
            switch (e.InputEventInfoType)
            {
                case InputEventInfoType.Key:
                    return e.Scancode != 0 ? $"{(KeyList)e.Scancode}" : $"{(KeyList)e.PhysicalScancode}";

                case InputEventInfoType.MouseButton:
                    if (_mouseButtons.ContainsKey(e.ButtonIndex))
                        return _mouseButtons[e.ButtonIndex];
                    
                    return $"{(ButtonList)e.ButtonIndex}";

                case InputEventInfoType.JoypadButton:
                    return $"{(JoystickList)e.ButtonIndex}";

                case InputEventInfoType.JoypadMotion:
                    return $"Joypad Axis {e.Axis} ({e.AxisValue})";
            }

            throw new InvalidOperationException($"Could not display {e.GetType()} as readable text because it is not a supported type");
        }

        public static InputEvent ConvertToInputEvent(this InputEventInfo e)
        {
            switch (e.InputEventInfoType)
            {
                case InputEventInfoType.Key:
                    return new InputEventKey
                    {
                        Alt = e.Alt,
                        Command = e.Command,
                        Control = e.Control,
                        Device = e.Device,
                        Echo = e.Echo,
                        Meta = e.Meta,
                        PhysicalScancode = e.PhysicalScancode,
                        Pressed = e.Pressed,
                        Scancode = e.Scancode,
                        Shift = e.Shift,
                        Unicode = e.Unicode
                    };
                case InputEventInfoType.MouseButton:
                    return new InputEventMouseButton
                    {
                        Alt = e.Alt,
                        Command = e.Command,
                        Control = e.Control,
                        Device = e.Device,
                        Meta = e.Meta,
                        Pressed = e.Pressed,
                        Shift = e.Shift,
                        ButtonIndex = e.ButtonIndex,
                        ButtonMask = e.ButtonMask,
                        Doubleclick = e.DoubleClick,
                        Factor = e.Factor,
                    };
                case InputEventInfoType.JoypadButton:
                    return new InputEventJoypadButton
                    {
                        Device = e.Device,
                        Pressed = e.Pressed,
                        ButtonIndex = e.ButtonIndex,
                        Pressure = e.Pressure
                    };
                case InputEventInfoType.JoypadMotion:
                    return new InputEventJoypadMotion
                    {
                        Device = e.Device,
                        Axis = e.Axis,
                        AxisValue = e.AxisValue
                    };
            }

            throw new InvalidOperationException($"Could not convert {e.GetType()} to InputEvent because it is not a supported InputEventInfo type.");
        }
    }
}