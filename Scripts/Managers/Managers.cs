global using Godot;
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

namespace GodotModules
{
	public class Managers : Node
	{
		[Export] protected readonly NodePath NodePathAudioStreamPlayer;
		[Export] protected readonly NodePath NodePathWebRequestList;
		[Export] protected readonly NodePath NodePathScenes;
		[Export] protected readonly NodePath NodePathConsole;
		[Export] protected readonly NodePath NodePathErrorNotifierManager;
		[Export] protected readonly NodePath NodePathMenuParticles;
		[Export] protected readonly NodePath NodePathPopups;
		[Export] protected readonly NodePath NodePathSceneManager;

		public OptionsManager ManagerOptions { get; private set; }
		public TokenManager ManagerToken { get; private set; }
		public NetworkManager ManagerNetwork { get; private set; }
		public SceneManager ManagerScene { get; private set; }
		public WebManager ManagerWeb { get; private set; }
		public MusicManager ManagerMusic { get; private set; }
		public ErrorNotifierManager ManagerErrorNotifier { get; private set; }
		public PopupManager ManagerPopup { get; private set; }
		public HotkeyManager ManagerHotkey { get; private set; }
		public ConsoleManager ManagerConsole { get; private set; }

		private Particles2D _menuParticles;
		private bool _ready;

		public override async void _Ready()
		{
			ManagerHotkey = new();
			ManagerOptions = new(ManagerHotkey);
			ManagerToken = new();
			ManagerWeb = new(new(GetNode<Node>(NodePathWebRequestList)), ManagerToken, ManagerOptions.Options.WebServerAddress);
			ManagerMusic = new(GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer), ManagerOptions);
			ManagerErrorNotifier = GetNode<ErrorNotifierManager>(NodePathErrorNotifierManager);
			ManagerPopup = new(GetNode<Node>(NodePathPopups), this);
			ManagerNetwork = new(ManagerPopup);
			ManagerConsole = GetNode<ConsoleManager>(NodePathConsole);

			ManagerHotkey.AddCategories("ui", "player", "camera");
			ManagerHotkey.Init();

			_menuParticles = GetNode<Particles2D>(NodePathMenuParticles);
			_menuParticles.Emitting = true;

			await InitSceneManager(GetNode<Control>(NodePathScenes), ManagerHotkey);

			Logger.UIConsole = ManagerConsole;
			Logger.ErrorNotifierManager = ManagerErrorNotifier;
			ModLoader.Init();

			UpdateParticleSystem();

			ManagerMusic.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
			ManagerMusic.PlayTrack("Menu");

			await ManagerWeb.CheckConnectionAsync();

			if (ManagerWeb.ConnectionAlive)
				await ManagerWeb.GetExternalIpAsync();

			_ready = true;
		}

		public async Task InitSceneManager(Control sceneList, HotkeyManager hotkeyManager)
		{
			ManagerScene = GetNode<SceneManager>(NodePathSceneManager);
			ManagerScene.Init(sceneList, hotkeyManager, this);

			// Custom Pre Init
			ManagerScene.PreInit[GameScene.Menu] = (node) =>
			{
				((SceneMenu)node).PreInit(_menuParticles);
			};

			// Esc Pressed
			ManagerScene.EscPressed[GameScene.Credits] = async () => await ManagerScene.ChangeScene(GameScene.Menu);
			ManagerScene.EscPressed[GameScene.GameServers] = async () => await ManagerScene.ChangeScene(GameScene.Menu);
			ManagerScene.EscPressed[GameScene.Mods] = async () => 
			{
				ModLoader.SceneMods = null;
				await ManagerScene.ChangeScene(GameScene.Menu);
			};
			ManagerScene.EscPressed[GameScene.Options] = async () =>
			{
				ManagerToken.Cancel("check_connection");
				await ManagerScene.ChangeScene(GameScene.Menu);
			};
			ManagerScene.EscPressed[GameScene.Lobby] = async () => await ManagerScene.ChangeScene(GameScene.GameServers);
			ManagerScene.EscPressed[GameScene.Game] = async () =>
			{
				await ManagerScene.ChangeScene(GameScene.Menu);
				_menuParticles.Emitting = true;
				_menuParticles.Visible = true;
			};

			await ManagerScene.InitAsync();
		}

		public override async void _Process(float delta)
		{
			Logger.Update();
			await ManagerNetwork.Update();
		}

		public override void _Input(InputEvent @event)
		{
			if (Input.IsActionJustPressed("ui_cancel"))
				if (ManagerConsole.Visible)
					ManagerConsole.ToggleVisibility();
				else if (ManagerScene.EscPressed.ContainsKey(ManagerScene.CurScene))
					ManagerScene.EscPressed[ManagerScene.CurScene]();

			if (Input.IsActionJustPressed("ui_fullscreen"))
				ManagerOptions.ToggleFullscreen();

			if (Input.IsActionJustPressed("ui_console"))
				ManagerConsole.ToggleVisibility();
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
			ModLoader.SaveEnabled();
			ManagerOptions.SaveOptions();
			await ManagerNetwork.Cleanup();
			ManagerToken.Cleanup();
			GetTree().Quit();
		}
	}
}
