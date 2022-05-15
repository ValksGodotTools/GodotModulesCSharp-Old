using Godot;
using System;

namespace GodotModules
{
    public class Game : Node
    {
        [Export] public readonly NodePath NodePathAudioStreamPlayer;
        [Export] public readonly NodePath NodePathWebRequestList;
        private AudioStreamPlayer _audioStreamPlayer;
        private Node _webRequestList;

        private GM _gm;
        private HotkeyManager _hotkeyManager;
        private SystemFileManager _systemFileManager;
        private SceneManager _sceneManager;
        private OptionsManager _optionsManager;
        private MusicManager _musicManager;
        private NetworkManager _networkManager;

        public override async void _Ready()
        {
            _audioStreamPlayer = GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer);
            _webRequestList = GetNode<Node>(NodePathWebRequestList);

            _systemFileManager = new();
            _hotkeyManager = new(_systemFileManager);
            _optionsManager = new(_systemFileManager, _hotkeyManager);
            _musicManager = new(_audioStreamPlayer, _optionsManager);
            _musicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            _musicManager.PlayTrack("Menu");

            _sceneManager = new(this, new GodotFileManager(), _hotkeyManager);
            _sceneManager.PreInit[Scene.Options] = (scene) =>
            {
                var options = (UIOptions)scene;
                options.PreInit(_hotkeyManager, _optionsManager, _musicManager);
            };
            _sceneManager.EscPressed[Scene.Credits] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.GameServers] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Mods] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Options] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            _sceneManager.EscPressed[Scene.Lobby] = async () => await _sceneManager.ChangeScene(Scene.GameServers);
            _sceneManager.EscPressed[Scene.Game] = async () => await _sceneManager.ChangeScene(Scene.Menu);
            await _sceneManager.InitAsync();
            _gm = new GM(_sceneManager);

            _networkManager = new(_webRequestList);
            _networkManager.StartServer(25565, 100);
            _networkManager.StartClient("127.0.0.1", 25565);
            await _networkManager.Setup();
        }

        public override async void _Process(float delta)
        {
            _gm.Update();
            await _networkManager.Update();
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
            await _networkManager.Cleanup();
            GetTree().Quit();
        }
    }
}
