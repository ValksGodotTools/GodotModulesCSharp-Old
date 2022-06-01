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
            await _managers.ManagerScene.ChangeScene(GameScene.Game);
            _menuParticles.Emitting = false;
            _menuParticles.Visible = false;
        }

        private async void _on_Multiplayer_pressed() 
        {
            if (!_managers.Net.EnetInitialized) 
            {
                var message = "Multiplayer is disabled because ENet failed to initialize";
                Logger.LogWarning(message);
                _managers.Popups.SpawnMessage(message);
                return;
            }

            if (string.IsNullOrWhiteSpace(_managers.Options.Data.OnlineUsername))
            {
                _managers.Popups.SpawnLineEdit(
                    lineEdit => lineEdit.Filter((text) => Regex.IsMatch(text, "^[a-z]+$")), 
                    result => _managers.Options.Data.OnlineUsername = result, 
                    "Set Online Username", 20);
                return;
            }

            await _managers.ManagerScene.ChangeScene(GameScene.GameServers);
        }

        private async void _on_3D_Test_pressed()
        {
            await _managers.ManagerScene.ChangeScene(GameScene.Game3D);
            _menuParticles.Emitting = false;
            _menuParticles.Visible = false;
        }
        private async void _on_Options_pressed() => await _managers.ManagerScene.ChangeScene(GameScene.Options);
        private async void _on_Mods_pressed() => await _managers.ManagerScene.ChangeScene(GameScene.Mods);
        private async void _on_Credits_pressed() => await _managers.ManagerScene.ChangeScene(GameScene.Credits);
        private void _on_Quit_pressed() => GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        private void _on_Discord_pressed() => OS.ShellOpen("https://discord.gg/866cg8yfxZ");
        private void _on_GitHub_pressed() => OS.ShellOpen("https://github.com/GodotModules/GodotModulesCSharp");
    }
}
