using Godot;
using Valk.Modules.Settings;
using Valk.Modules;

namespace Game
{
    // DEMO
    public class UIMenu : Node
    {
        public override void _Ready()
        {
            MusicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            MusicManager.PlayTrack("Menu");
        }

        private void _on_Play_pressed() => GetTree().ChangeScene("res://Scenes/Game.tscn");
        private void _on_Multiplayer_pressed() {}
        private void _on_Exit_pressed() => GameManager.Exit();
    }
}
