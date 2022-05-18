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

                case (int)ButtonList.Middle:
                    return "Button 3";

                case (int)ButtonList.Right:
                    return "Right Mouse";
            }

            return m.AsText();
        }
    }
}