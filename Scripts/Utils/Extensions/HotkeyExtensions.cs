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
                    return "Left Mouse";

                case (int)ButtonList.Right:
                    return "Right Mouse";

                case (int)ButtonList.Middle:
                    return "Mouse 3";

                case (int)ButtonList.Xbutton1:
                    return "Mouse 4";

                case (int)ButtonList.Xbutton2:
                    return "Mouse 5";
            }

            return m.AsText();
        }
    }
}