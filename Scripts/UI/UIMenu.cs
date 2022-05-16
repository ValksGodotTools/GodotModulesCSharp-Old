using Godot;

namespace GodotModules
{
    public class UIMenu : Node
    {
        [Export] public readonly NodePath NodePathBtnPlay;

        private SceneManager _sceneManager;
        private NetworkManager _networkManager;

        public void PreInit(SceneManager sceneManager, NetworkManager networkManager) 
        {
            _sceneManager = sceneManager;
            _networkManager = networkManager;
        }

        public override void _Ready() => GetNode<Button>(NodePathBtnPlay).GrabFocus();

        private async void _on_Play_pressed() => await _sceneManager.ChangeScene(Scene.Game);
        private async void _on_Multiplayer_pressed() 
        {
            if (!_networkManager.EnetInitialized) 
            {
                Logger.LogWarning("Multiplayer is disabled because ENet failed to initialize");
                return;
            }

            await _sceneManager.ChangeScene(Scene.GameServers);
        }
        private async void _on_Options_pressed() => await _sceneManager.ChangeScene(Scene.Options);
        private async void _on_Mods_pressed() => await _sceneManager.ChangeScene(Scene.Mods);
        private async void _on_Credits_pressed() => await _sceneManager.ChangeScene(Scene.Credits);
        private void _on_Quit_pressed() => GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        private void _on_Discord_pressed() => OS.ShellOpen("https://discord.gg/866cg8yfxZ");
        private void _on_GitHub_pressed() => OS.ShellOpen("https://github.com/GodotModules/GodotModulesCSharp");
    }
}
