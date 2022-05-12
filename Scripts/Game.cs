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
            _sceneManager = new(this, new GodotFileManager());
            _gm = new GM(this, _sceneManager);
            _systemFileManager = new();
            _hotkeyManager = new(_systemFileManager);

            await _sceneManager.InitAsync(_hotkeyManager);
            GM.Net.StartServer(25565, 100);
            GM.Net.StartClient("127.0.0.1", 25565);
            await GM.Net.WebClient.CheckConnectionAsync();
        }

        public override async void _Process(float delta)
        {
            await _gm.Update();
        }

        public override void _Input(InputEvent @event)
        {
            _sceneManager.IfEscapePressed(async () =>
            {
                await GM.ChangeScene(_sceneManager.PrevSceneName);
            });
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
