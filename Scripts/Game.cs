using Godot;
using System;

namespace GodotModules
{
    public class Game : Node
    {
        private GM _gm;
        private HotkeyManager _hotkeyManager;
        private SystemFileManager _systemFileManager;

        public override async void _Ready()
        {
            _gm = new GM(this);
            _systemFileManager = new();
            _hotkeyManager = new(_systemFileManager);

            await GM.SceneManager.InitAsync(_hotkeyManager);
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
            GM.SceneManager.IfEscapePressed(async () =>
            {
                await GM.ChangeScene(GM.SceneManager.PrevSceneName);
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
