using Godot;
using System;

namespace GodotModules
{
    public class Game : Node
    {
        private GM _gm;
        private HotkeyManager _hotkeyManager;
        private SystemFileManager _systemFileManager;
        private SceneManager _sceneManager;

        public override async void _Ready()
        {
            _systemFileManager = new();
            _hotkeyManager = new(_systemFileManager);
            _sceneManager = new(this, new GodotFileManager(), _hotkeyManager);
            
            _sceneManager.PreInit(Scene.Menu, (scene) =>
            {
                var menu = (UIMenu)scene;
                menu.HotkeyManager = _hotkeyManager;
            });

            _sceneManager.PreInit(Scene.Options, (scene) =>
            {
                var options = (UIOptions)scene;
                options.PreInit(_hotkeyManager);
            });

            _gm = new GM(_sceneManager);

            await _sceneManager.InitAsync();
            GM.Net.StartServer(25565, 100);
            GM.Net.StartClient("127.0.0.1", 25565);
            await GM.Net.WebClient.CheckConnectionAsync();
        }

        public override async void _Process(float delta)
        {
            await _gm.Update();
        }

        public override async void _Input(InputEvent @event)
        {
            await GM.IfEscGoToPrevScene();
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GetTree().SetAutoAcceptQuit(false);
                await Cleanup();
            }
        }

        private async Task Cleanup()
        {
            _hotkeyManager.SaveHotkeys();
            await GM.Net.Cleanup();
            GetTree().Quit();
        }
    }
}
