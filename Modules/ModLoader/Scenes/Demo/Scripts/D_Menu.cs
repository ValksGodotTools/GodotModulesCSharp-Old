using Godot;
using Valk.ModLoader;

namespace D_Game
{
    // DEMO
    public class D_Menu : Node
    {
        public override void _Ready()
        {
            ModLoader.LuaScriptsPath = "Modules/ModLoader/Scripts/Lua";
            ModLoader.ModsProjectPath = "Modules/ModLoader/Scenes/Demo";
            ModLoader.Init();
            ModLoader.LoadMods();

            UIModLoader.DisplayMods();
        }

        private void _on_Play_pressed()
        {
            GetTree().ChangeScene("res://Modules/ModLoader/Scenes/Demo/Scenes/D_Main.tscn");
        }
    }
}
