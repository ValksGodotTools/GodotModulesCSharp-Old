using Godot;
using System;

namespace GodotModules
{
    public class Game : Node
    {
        private GM _gm;

        public override async void _Ready()
        {
            _gm = new GM(this);

            await GM._sceneManager.InitAsync(_gm._hotkeyManager);
            GM.Net.StartServer(25565, 100);
            GM.Net.StartClient("127.0.0.1", 25565);
            await GM.Net.WebClient.CheckConnectionAsync();
        }

        public override async void _Process(float delta)
        {
            GM._logger.Update();
            await GM.Net.Update();
        }

        public override void _Input(InputEvent @event)
        {
            GM._sceneManager.IfEscapePressed(async () =>
            {
                await GM._sceneManager.ChangeScene(GM._sceneManager.PrevSceneName);
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
            _gm._hotkeyManager.SaveHotkeys();
            await GM.Net.Cleanup();
            GetTree().Quit();
        }
    }
}
