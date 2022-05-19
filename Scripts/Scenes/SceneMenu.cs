using Godot;

namespace GodotModules
{
    public class SceneMenu : AScene
    {
        [Export] protected readonly NodePath NodePathBtnPlay;

        private Managers _managers;
        private Particles2D _menuParticles;

        public void PreInit(Particles2D menuParticles) 
        {
            _menuParticles = menuParticles;
        }
        
        public override void PreInitManagers(Managers managers) => _managers = managers;

        public override void _Ready() => GetNode<Button>(NodePathBtnPlay).GrabFocus();

        private async void _on_Play_pressed() 
        {
            _menuParticles.Emitting = false;
            _menuParticles.Visible = false;
            await _managers.Scene.ChangeScene(GameScene.Game);
        }

        private async void _on_Multiplayer_pressed() 
        {
            if (!_managers.Network.EnetInitialized) 
            {
                var message = "Multiplayer is disabled because ENet failed to initialize";
                Logger.LogWarning(message);
                _managers.Popup.SpawnMessage(message);
                return;
            }

            if (string.IsNullOrWhiteSpace(_managers.Options.Options.OnlineUsername))
            {
                _managers.Popup.SpawnLineEdit(
                    (lineEdit) => lineEdit.Filter((text) => Regex.IsMatch(text, "^[a-z]+$")), 
                    (result) => _managers.Options.Options.OnlineUsername = result, 
                    20, "Set Online Username");
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
