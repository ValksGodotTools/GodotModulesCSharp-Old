using Godot;

namespace GodotModules
{
    public class SceneMenu : AScene
    {
        [Export] public readonly NodePath NodePathBtnPlay;

        private Managers _managers;
        
        public override void PreInit(Managers managers) => _managers = managers;

        public override void _Ready() => GetNode<Button>(NodePathBtnPlay).GrabFocus();

        private async void _on_Play_pressed() => await _managers.Scene.ChangeScene(GameScene.Game);
        private async void _on_Multiplayer_pressed() 
        {
            if (!_managers.Network.EnetInitialized) 
            {
                var message = "Multiplayer is disabled because ENet failed to initialize";
                Logger.LogWarning(message);
                _managers.Popup.SpawnPopupMessage(message);
                return;
            }

            await _managers.Scene.ChangeScene(GameScene.GameServers);
        }
        private async void _on_Options_pressed() => await _managers.Scene.ChangeScene(GameScene.Options);
        private async void _on_Mods_pressed() => await _managers.Scene.ChangeScene(GameScene.Mods);
        private async void _on_Credits_pressed() => await _managers.Scene.ChangeScene(GameScene.Credits);
        private void _on_Quit_pressed() => GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        private void _on_Discord_pressed() => OS.ShellOpen("https://discord.gg/866cg8yfxZ");
        private void _on_GitHub_pressed() => OS.ShellOpen("https://github.com/GodotModules/GodotModulesCSharp");
    }
}
