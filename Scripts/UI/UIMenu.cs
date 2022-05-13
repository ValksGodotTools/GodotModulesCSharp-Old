using Godot;

namespace GodotModules
{
    public class UIMenu : Node
    {
        public HotkeyManager HotkeyManager { get; set; }

        private async void _on_Play_pressed() => await GM.ChangeScene(Scene.Game);
        private async void _on_Multiplayer_pressed() => await GM.ChangeScene(Scene.GameServers);
        private async void _on_Options_pressed() => await GM.ChangeScene(Scene.Options);
        private async void _on_Mods_pressed() => await GM.ChangeScene(Scene.Mods);
        private async void _on_Credits_pressed() => await GM.ChangeScene(Scene.Credits);
        private void _on_Quit_pressed() => GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        private void _on_Discord_pressed() => OS.ShellOpen("https://discord.gg/866cg8yfxZ");
        private void _on_GitHub_pressed() => OS.ShellOpen("https://github.com/GodotModules/GodotModulesCSharp");
    }
}
