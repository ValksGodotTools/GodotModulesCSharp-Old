using Godot;

namespace GodotModules 
{
    public static class HotkeyExtensions 
    {
        public static string Display(this InputEvent e)
        {
            if (e is InputEventKey k)
                return k.AsText();
            
            if (e is InputEventMouseButton m)
                return Display(m);
            
            if (e is InputEventJoypadButton j)
                return j.AsText();
            
            throw new InvalidOperationException("Unknown InputEvent type.");
        }

        private static string Display(InputEventMouseButton m)
        {
            switch (m.ButtonIndex)
            {
                case (int)ButtonList.Left:
                    return "Left Click";

                case (int)ButtonList.Right:
                    return "Right Click";

                case (int)ButtonList.Middle:
                    return "Middle Click";

                case (int)ButtonList.Xbutton1:
                    return "Mouse XBtn 1";

                case (int)ButtonList.Xbutton2:
                    return "Mouse XBtn 2";

                case (int)ButtonList.WheelDown:
                    return "Wheel Down";

                case (int)ButtonList.WheelUp:
                    return "Wheel Up";

                case (int)ButtonList.WheelLeft:
                    return "Wheen Left";

                case (int)ButtonList.WheelRight:
                    return "Wheel Right";
            }

            return m.AsText();
        }
    }
}