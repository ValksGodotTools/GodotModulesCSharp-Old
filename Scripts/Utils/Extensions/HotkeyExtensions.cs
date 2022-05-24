using Godot;

namespace GodotModules 
{
    public static class HotkeyExtensions 
    {
        private static readonly Dictionary<int, string> _mouseButtons = new()
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

        public static string Display(this InputEvent e)
        {
            if (e is InputEventMouseButton m && _mouseButtons.ContainsKey(m.ButtonIndex))
                return _mouseButtons[m.ButtonIndex];
            
            return e.AsText();
        }
    }
}