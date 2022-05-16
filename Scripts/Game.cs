global using GodotModules;
global using GodotModules.Netcode;
global using System;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;
global using System.Linq;

using Godot;

namespace GodotModules
{
    public class Game : Node
    {
        [Export] public readonly NodePath NodePathAudioStreamPlayer;
        [Export] public readonly NodePath NodePathWebRequestList;
        [Export] public readonly NodePath NodePathScenes;
        [Export] public readonly NodePath NodePathConsole;
        [Export] public readonly NodePath NodePathErrorNotifierManager;
        [Export] public readonly NodePath NodePathPopupManager;

        private Managers _managers;

        public override async void _Ready()
        {
            _managers = new(GetNode<Node>(NodePathWebRequestList), GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer), 
                GetNode<ErrorNotifierManager>(NodePathErrorNotifierManager), GetNode<PopupManager>(NodePathPopupManager),
                GetNode<ConsoleManager>(NodePathConsole));
            await _managers.InitSceneManager(GetNode<Control>(NodePathScenes), _managers.HotkeyManager);
            
            // how else would you pass this information to Logger?
            Logger.UIConsole = _managers.ConsoleManager;
            Logger.ErrorNotifierManager = _managers.ErrorNotifierManager;

            _managers.MusicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            _managers.MusicManager.PlayTrack("Menu");

            _managers.NetworkManager.StartServer(25565, 100);
            _managers.NetworkManager.StartClient("127.0.0.1", 25565);

            await _managers.WebManager.CheckConnectionAsync();
            if (_managers.WebManager.ConnectionAlive)
                await _managers.WebManager.GetExternalIpAsync();
        }

        public override async void _Process(float delta)
        {
            Logger.Update();
            await _managers.NetworkManager.Update();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                if (_managers.ConsoleManager.Visible)
                    _managers.ConsoleManager.ToggleVisibility();
                else if (_managers.SceneManager.EscPressed.ContainsKey(_managers.SceneManager.CurScene))
                    _managers.SceneManager.EscPressed[_managers.SceneManager.CurScene]();

            if (Input.IsActionJustPressed("ui_console"))
                _managers.ConsoleManager.ToggleVisibility();
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
            _managers.OptionsManager.SaveOptions();
            await _managers.NetworkManager.Cleanup();
            _managers.TokenManager.Cleanup();
            GetTree().Quit();
        }
    }

    public class Managers 
    {
        public OptionsManager OptionsManager { get; private set; }
        public TokenManager TokenManager;
        public NetworkManager NetworkManager;
        public SceneManager SceneManager;
        public WebManager WebManager;
        public MusicManager MusicManager;
        public ErrorNotifierManager ErrorNotifierManager;
        public PopupManager PopupManager;
        public HotkeyManager HotkeyManager;
        public ConsoleManager ConsoleManager;

        public Managers(Node webRequestList, AudioStreamPlayer audioStreamPlayer, ErrorNotifierManager errorNotifierManager, PopupManager popupManager, ConsoleManager consoleManager)
        {
            var systemFileManager = new SystemFileManager();
            HotkeyManager = new HotkeyManager(systemFileManager, new List<string>() {"UI", "Player"});
            OptionsManager = new(systemFileManager, HotkeyManager);
            TokenManager = new();
            WebManager = new(new WebRequests(webRequestList), TokenManager, OptionsManager.Options.WebServerAddress);
            MusicManager = new(audioStreamPlayer, OptionsManager);
            
            ErrorNotifierManager = errorNotifierManager;
            PopupManager = popupManager;
            NetworkManager = new(PopupManager);
            ConsoleManager = consoleManager;
        }

        public async Task InitSceneManager(Control sceneList, HotkeyManager hotkeyManager)
        {
            SceneManager = new(sceneList, new GodotFileManager(), hotkeyManager);

            // Pre Initialization
            SceneManager.PreInit[Scene.Menu] = (scene) =>
            {
                var menu = (UIMenu)scene;
                menu.PreInit(SceneManager, NetworkManager, PopupManager);
            };
            SceneManager.PreInit[Scene.Options] = (scene) =>
            {
                var options = (UIOptions)scene;
                options.PreInit(hotkeyManager, OptionsManager, MusicManager, WebManager, SceneManager, TokenManager);
            };
            SceneManager.PreInit[Scene.Credits] = (scene) =>
            {
                var credits = (UICredits)scene;
                credits.PreInit(SceneManager);
            };

            // Esc Pressed
            SceneManager.EscPressed[Scene.Credits] = async () => await SceneManager.ChangeScene(Scene.Menu);
            SceneManager.EscPressed[Scene.GameServers] = async () => await SceneManager.ChangeScene(Scene.Menu);
            SceneManager.EscPressed[Scene.Mods] = async () => await SceneManager.ChangeScene(Scene.Menu);
            SceneManager.EscPressed[Scene.Options] = async () => {
                TokenManager.Cancel("check_connection");
                await SceneManager.ChangeScene(Scene.Menu);
            };
            SceneManager.EscPressed[Scene.Lobby] = async () => await SceneManager.ChangeScene(Scene.GameServers);
            SceneManager.EscPressed[Scene.Game] = async () => await SceneManager.ChangeScene(Scene.Menu);

            await SceneManager.InitAsync();
        }
    }
}
