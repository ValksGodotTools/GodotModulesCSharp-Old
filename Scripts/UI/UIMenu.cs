using Godot;

namespace GodotModules
{
    public class UIMenu : Node
    {
        public HotkeyManager HotkeyManager { get; set; }

        private async void _on_Play_pressed() => await GM.ChangeScene("Game");
        private async void _on_Multiplayer_pressed() => await GM.ChangeScene("GameServers");
        private async void _on_Options_pressed() => await GM.ChangeScene("Options", (scene) => {
            var options = (UIOptions)scene;
            options.Init(HotkeyManager);
        });
        private async void _on_Mods_pressed() => await GM.ChangeScene("Mods");
        private async void _on_Credits_pressed() => await GM.ChangeScene("Credits");
        private void _on_Quit_pressed() => GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        private void _on_Discord_pressed() => OS.ShellOpen("https://discord.gg/866cg8yfxZ");
        private void _on_GitHub_pressed() => OS.ShellOpen("https://github.com/GodotModules/GodotModulesCSharp");
    }
}
