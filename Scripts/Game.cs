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
        [Export] public readonly NodePath NodePathMenuParticles;
        [Export] public readonly NodePath NodePathPopups;

        private Managers _managers;
        private Particles2D _particles2D;
        private bool _ready;

        public override async void _Ready()
        {
            _particles2D = GetNode<Particles2D>(NodePathMenuParticles);
            _particles2D.Emitting = true;

            _managers = new
            (
                _particles2D,
                GetNode<Node>(NodePathWebRequestList),
                GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer), 
                GetNode<ErrorNotifierManager>(NodePathErrorNotifierManager),
                GetNode<Node>(NodePathPopups),
                GetNode<ConsoleManager>(NodePathConsole)
            );

            await _managers.InitSceneManager(GetNode<Control>(NodePathScenes), _managers.Hotkey);
                        
            // how else would you pass this information to Logger?
            Logger.UIConsole = _managers.Console;
            Logger.ErrorNotifierManager = _managers.ErrorNotifier;

            _managers.Music.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            _managers.Music.PlayTrack("Menu");

            _managers.Network.StartServer(25565, 100);
            _managers.Network.StartClient("127.0.0.1", 25565);

            await Task.Delay(10);
            UpdateParticleSystem();
            _ready = true;

            await _managers.Web.CheckConnectionAsync();

            if (_managers.Web.ConnectionAlive)
                await _managers.Web.GetExternalIpAsync();
        }

        public override async void _Process(float delta)
        {
            Logger.Update();
            await _managers.Network.Update();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                if (_managers.Console.Visible)
                    _managers.Console.ToggleVisibility();
                else if (_managers.Scene.EscPressed.ContainsKey(_managers.Scene.CurScene))
                    _managers.Scene.EscPressed[_managers.Scene.CurScene]();

            if (Input.IsActionJustPressed("ui_fullscreen"))
                _managers.Options.ToggleFullscreen();

            if (Input.IsActionJustPressed("ui_console"))
                _managers.Console.ToggleVisibility();
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GetTree().SetAutoAcceptQuit(false);
                await Cleanup();
            }
        }

        private void _on_Scenes_resized() 
        {
            if (_ready)
                UpdateParticleSystem();
        }

        private void UpdateParticleSystem()
        {
            _particles2D.Position = new Vector2(OS.WindowSize.x / 2, OS.WindowSize.y);
            _particles2D.ProcessMaterial.SetIndexed("emission_box_extents:x", OS.WindowSize.x / 2);
        }

        private async Task Cleanup()
        {
            _managers.Options.SaveOptions();
            await _managers.Network.Cleanup();
            _managers.Token.Cleanup();
            GetTree().Quit();
        }
    }

    public class Managers 
    {
        public OptionsManager Options { get; private set; }
        public TokenManager Token { get; private set; }
        public NetworkManager Network { get; private set; }
        public SceneManager Scene { get; private set; }
        public WebManager Web { get; private set; }
        public MusicManager Music { get; private set; }
        public ErrorNotifierManager ErrorNotifier { get; private set; }
        public PopupManager Popup { get; private set; }
        public HotkeyManager Hotkey { get; private set; }
        public ConsoleManager Console { get; private set; }

        private Particles2D _menuParticles;

        public Managers(Particles2D menuParticles, Node webRequestList, AudioStreamPlayer audioStreamPlayer, ErrorNotifierManager errorNotifierManager, Node popups, ConsoleManager consoleManager)
        {
            _menuParticles = menuParticles;

            var systemFileManager = new SystemFileManager();
            Hotkey = new(systemFileManager, new List<string>() {"UI", "Player", "Camera"});
            Options = new(systemFileManager, Hotkey);
            Token = new();
            Web = new(new WebRequests(webRequestList), Token, Options.Options.WebServerAddress);
            Music = new(audioStreamPlayer, Options);
            
            ErrorNotifier = errorNotifierManager;
            Popup = new(popups);
            Network = new(Popup);
            Console = consoleManager;
        }

        public async Task InitSceneManager(Control sceneList, HotkeyManager hotkeyManager)
        {
            Scene = new(sceneList, new GodotFileManager(), hotkeyManager, this);

            // Custom Pre Init
            Scene.PreInit[GameScene.Menu] = (node) => {
                Logger.Log("PREINIT: " + _menuParticles);
                ((SceneMenu)node).PreInit(_menuParticles);
            };

            // Esc Pressed
            Scene.EscPressed[GameScene.Credits] = async () => await Scene.ChangeScene(GameScene.Menu);
            Scene.EscPressed[GameScene.GameServers] = async () => await Scene.ChangeScene(GameScene.Menu);
            Scene.EscPressed[GameScene.Mods] = async () => await Scene.ChangeScene(GameScene.Menu);
            Scene.EscPressed[GameScene.Options] = async () =>
            {
                Token.Cancel("check_connection");
                await Scene.ChangeScene(GameScene.Menu);
            };
            Scene.EscPressed[GameScene.Lobby] = async () => await Scene.ChangeScene(GameScene.GameServers);
            Scene.EscPressed[GameScene.Game] = async () => {
                await Scene.ChangeScene(GameScene.Menu);
                _menuParticles.Emitting = true;
                _menuParticles.Visible = true;
            };

            await Scene.InitAsync();
        }
    }
}
