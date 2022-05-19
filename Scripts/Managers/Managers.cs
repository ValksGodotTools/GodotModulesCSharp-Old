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
    public class Managers : Node
    {
        public static Managers Instance { get; private set; }

        [Export] protected readonly NodePath NodePathAudioStreamPlayer;
        [Export] protected readonly NodePath NodePathWebRequestList;
        [Export] protected readonly NodePath NodePathScenes;
        [Export] protected readonly NodePath NodePathConsole;
        [Export] protected readonly NodePath NodePathErrorNotifierManager;
        [Export] protected readonly NodePath NodePathMenuParticles;
        [Export] protected readonly NodePath NodePathPopups;

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
        private bool _ready;

        public override async void _Ready()
        {
            Instance = this;

            var systemFileManager = new SystemFileManager();
            Hotkey = new(systemFileManager, new List<string>() {"UI", "Player", "Camera"});
            Options = new(systemFileManager, Hotkey);
            Token = new();
            Web = new(new WebRequests(GetNode<Node>(NodePathWebRequestList)), Token, Options.Options.WebServerAddress);
            Music = new(GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer), Options);
            ErrorNotifier = GetNode<ErrorNotifierManager>(NodePathErrorNotifierManager);
            Popup = new(GetNode<Node>(NodePathPopups));
            Network = new(Popup);
            Console = GetNode<ConsoleManager>(NodePathConsole);

            _menuParticles = GetNode<Particles2D>(NodePathMenuParticles);
            _menuParticles.Emitting = true;

            await InitSceneManager(GetNode<Control>(NodePathScenes), Hotkey);
                        
            Logger.UIConsole = Console;
            Logger.ErrorNotifierManager = ErrorNotifier;

            UpdateParticleSystem();

            Music.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
            Music.PlayTrack("Menu");

            Network.StartServer(25565, 100);
            Network.StartClient("127.0.0.1", 25565);

            await Web.CheckConnectionAsync();

            if (Web.ConnectionAlive)
                await Web.GetExternalIpAsync();

            _ready = true;
        }

        public async Task InitSceneManager(Control sceneList, HotkeyManager hotkeyManager)
        {
            Scene = new(sceneList, new GodotFileManager(), hotkeyManager, this);

            // Custom Pre Init
            Scene.PreInit[GameScene.Menu] = (node) => {
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

        public override async void _Process(float delta)
        {
            Logger.Update();
            await Network.Update();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                if (Console.Visible)
                    Console.ToggleVisibility();
                else if (Scene.EscPressed.ContainsKey(Scene.CurScene))
                    Scene.EscPressed[Scene.CurScene]();

            if (Input.IsActionJustPressed("ui_fullscreen"))
                Options.ToggleFullscreen();

            if (Input.IsActionJustPressed("ui_console"))
                Console.ToggleVisibility();
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
            _menuParticles.Position = new Vector2(OS.WindowSize.x / 2, OS.WindowSize.y);
            _menuParticles.ProcessMaterial.SetIndexed("emission_box_extents:x", OS.WindowSize.x / 2);
        }

        private async Task Cleanup()
        {
            Options.SaveOptions();
            await Network.Cleanup();
            Token.Cleanup();
            GetTree().Quit();
        }
    }
}
