using Godot;

namespace D_Game
{
    // DEMO
    public class D_Menu : Node
    {
        private void _on_Play_pressed()
        {
            GetTree().ChangeScene("res://Modules/ModLoader/Scenes/Demo/Scenes/D_Main.tscn");
        }
    }
}
