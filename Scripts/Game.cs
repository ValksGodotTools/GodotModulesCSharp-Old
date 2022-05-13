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
        private OptionsManager _optionsManager;

        public override async void _Ready()
        {
            _systemFileManager = new();
            _hotkeyManager = new(_systemFileManager);
            _optionsManager = new(_systemFileManager, _hotkeyManager);
            _sceneManager = new(this, new GodotFileManager(), _hotkeyManager);

            _sceneManager.PreInit[Scene.Options] = (scene) =>
            {
                var options = (UIOptions)scene;
                options.PreInit(_hotkeyManager, _optionsManager);
            };

            _sceneManager.EscPressed[Scene.Credits] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.GameServers] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Mods] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Options] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Lobby] = async () => await _sceneManager.ChangeScene(Scene.GameServers);
            _sceneManager.EscPressed[Scene.Game] = async () => await _sceneManager.ChangeScene(Scene.Menu);

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

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                if (_sceneManager.EscPressed.ContainsKey(_sceneManager.CurScene))
                    _sceneManager.EscPressed[_sceneManager.CurScene]();
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
            _optionsManager.SaveOptions();
            await GM.Net.Cleanup();
            GetTree().Quit();
        }
    }
}
