using Godot;
using Valk.Modules.Settings;
using Valk.Modules;

namespace D_Game
{
    // DEMO
    public class D_Menu : Node
    {
        public override void _Ready()
        {
            MusicManager.LoadTrack("Menu", "Modules/ModLoader/Scenes/Demo/Audio/Music/Unsolicited trailer music loop edit.wav");
            MusicManager.PlayTrack("Menu");
        }

        private void _on_Play_pressed() => GetTree().ChangeScene("res://Modules/ModLoader/Scenes/Demo/Scenes/D_Main.tscn");
        private void _on_Exit_pressed() => GameManager.Exit();
    }
}
