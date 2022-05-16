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
		[Export] public readonly NodePath NodePathConsole;
		[Export] public readonly NodePath NodePathErrorNotifierManager;
		[Export] public readonly NodePath NodePathPopupManager;

		private UIConsole _console;
		
		[Inject] private OptionsManager _optionsManager;
		[Inject] private TokenManager _tokenManager;
		[Inject] private NetworkManager _networkManager;
		[Inject] private WebManager _webManager;
		[Inject] private MusicManager _musicManager;
		[Inject] private ErrorNotifierManager _errorNotifierManager;
		[Inject] private PopupManager _popupManager;
		[Inject] private HotkeyManager _hotkeyManager;

		// Cannot inject this! (Initialized afterwards)
		private SceneManager _sceneManager;

		public override async void _Ready()
		{
			await InitManagers();
			_console = GetNode<UIConsole>(NodePathConsole);
			
			// how else would you pass this information to Logger?
			Logger.UIConsole = _console;
			Logger.ErrorNotifierManager = _errorNotifierManager;

			_musicManager.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
			_musicManager.PlayTrack("Menu");

			_networkManager.StartServer(25565, 100);
			_networkManager.StartClient("127.0.0.1", 25565);

			await _webManager.CheckConnectionAsync();
			if (_webManager.ConnectionAlive)
				await _webManager.GetExternalIpAsync();
		}

		public override async void _Process(float delta)
		{
			Logger.Update();
			await _networkManager.Update();
		}

		public override void _Input(InputEvent @event)
		{
			if (Input.IsActionJustPressed("ui_cancel"))
				if (_console.Visible)
					_console.ToggleVisibility();
				else if (_sceneManager.EscPressed.ContainsKey(_sceneManager.CurScene))
					_sceneManager.EscPressed[_sceneManager.CurScene]();

			if (Input.IsActionJustPressed("ui_console"))
				_console.ToggleVisibility();
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
			_tokenManager.Cleanup();
			GetTree().Quit();
		}

		private async Task InitManagers()
		{
			// Register Managers
			Container.Register<GodotFileManager>(new());
			Container.Register<ErrorNotifierManager>(GetNode<ErrorNotifierManager>(NodePathErrorNotifierManager));
			Container.Register<PopupManager>(GetNode<PopupManager>(NodePathPopupManager));

			Container.InjectAndRegister<SystemFileManager>(new());
			Container.InjectAndRegister<HotkeyManager>(new(new List<string>() {"UI", "Player"}));
			Container.InjectAndRegister<OptionsManager>(new());
			Container.InjectAndRegister<TokenManager>(new());
			Container.InjectAndRegister<WebManager>(new(new WebRequests(GetNode<Node>(NodePathWebRequestList)), Container.Get<OptionsManager>().Options.WebServerAddress));
			Container.InjectAndRegister<MusicManager>(new(GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer)));
			Container.InjectAndRegister<NetworkManager>(new());
			
			// Inject Managers
			Container.Inject(this);

			// Init SceneManager
			await InitSceneManager(_hotkeyManager);
		}

		private async Task InitSceneManager(HotkeyManager hotkeyManager)
		{
			_sceneManager = Container.InjectAndRegister<SceneManager>(new(this));

			// Esc Pressed
			_sceneManager.EscPressed[Scene.Credits] = async () => await _sceneManager.ChangeScene(Scene.Menu);
			_sceneManager.EscPressed[Scene.GameServers] = async () => await _sceneManager.ChangeScene(Scene.Menu);
			_sceneManager.EscPressed[Scene.Mods] = async () => await _sceneManager.ChangeScene(Scene.Menu);
			_sceneManager.EscPressed[Scene.Options] = async () => {
				_tokenManager.Cancel("check_connection");
				await _sceneManager.ChangeScene(Scene.Menu);
			};
			_sceneManager.EscPressed[Scene.Lobby] = async () => await _sceneManager.ChangeScene(Scene.GameServers);
			_sceneManager.EscPressed[Scene.Game] = async () => await _sceneManager.ChangeScene(Scene.Menu);

			await _sceneManager.InitAsync();
		}
	}
}
