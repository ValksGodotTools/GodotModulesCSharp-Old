using Godot;

namespace GodotModules
{
    public class UIMenu : Node
    {
        [Export] public readonly NodePath NodePathBtnPlay;

        private SceneManager _sceneManager;
        private NetworkManager _networkManager;
        private PopupManager _popupManager;

        public void PreInit(SceneManager sceneManager, NetworkManager networkManager, PopupManager popupManager) 
        {
            _sceneManager = sceneManager;
            _networkManager = networkManager;
            _popupManager = popupManager;
        }

        public override void _Ready() => GetNode<Button>(NodePathBtnPlay).GrabFocus();

        private async void _on_Play_pressed() => await _sceneManager.ChangeScene(Scene.Game);
        private async void _on_Multiplayer_pressed() 
        {
            if (!_networkManager.EnetInitialized) 
            {
                var message = "Multiplayer is disabled because ENet failed to initialize";
                Logger.LogWarning(message);
                _popupManager.SpawnPopupMessage(message);
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
