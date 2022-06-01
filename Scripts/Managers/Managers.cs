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

		public Options Options { get; private set; }
		public Tokens Tokens { get; private set; }
		public Net Net { get; private set; }
		public SceneManager ManagerScene { get; private set; }
		public Web Web { get; private set; }
		public Music Music { get; private set; }
		public ErrorNotifier ErrorNotifier { get; private set; }
		public Popups Popups { get; private set; }
		public HotkeyManager ManagerHotkey { get; private set; }
		public ConsoleManager ManagerConsole { get; private set; }

		public Particles2D MenuParticles;
		private bool _ready;

		public override async void _Ready()
		{
			ManagerHotkey = new();
			Options = new(ManagerHotkey);
			Tokens = new();
			Web = new(new(GetNode<Node>(NodePathWebRequestList)), Tokens, Options.Data.WebServerAddress);
			Music = new(GetNode<AudioStreamPlayer>(NodePathAudioStreamPlayer), Options);
			ErrorNotifier = GetNode<ErrorNotifier>(NodePathErrorNotifierManager);
			Popups = new(GetNode<Node>(NodePathPopups), this);
			Net = new(this);
			ManagerConsole = GetNode<ConsoleManager>(NodePathConsole);

			ManagerHotkey.AddCategories("ui", "player", "camera");
			ManagerHotkey.Init();

			MenuParticles = GetNode<Particles2D>(NodePathMenuParticles);
			MenuParticles.Emitting = true;

			await InitSceneManager(GetNode<Control>(NodePathScenes), ManagerHotkey);

			Logger.UIConsole = ManagerConsole;
			Logger.ErrorNotifierManager = ErrorNotifier;
			ModLoader.Init();

			Items.Init();

			UpdateParticleSystem();

			Music.LoadTrack("Menu", "Audio/Music/Unsolicited trailer music loop edit.wav");
			Music.PlayTrack("Menu");

			await Web.CheckConnectionAsync();

			if (Web.ConnectionAlive)
				await Web.GetExternalIpAsync();

			_ready = true;
		}

		public async Task InitSceneManager(Control sceneList, HotkeyManager hotkeyManager)
		{
			ManagerScene = GetNode<SceneManager>(NodePathSceneManager);
			ManagerScene.Init(sceneList, hotkeyManager, this);

			// Custom Pre Init
			ManagerScene.PreInit[GameScene.Menu] = (node) => ((SceneMenu)node).PreInit(MenuParticles);

			await ManagerScene.InitAsync();
		}

		public override async void _Process(float delta)
		{
			Logger.Update();
			await Net.Update();
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey inputEventKey)
				Notifications.Notify(this, Event.OnKeyboardInput, inputEventKey);

			if (@event is InputEventMouseButton inputEventMouseButton)
				Notifications.Notify(this, Event.OnMouseButtonInput, inputEventMouseButton);

			if (@event is InputEventMouseMotion inputEventMouseMotion)
				Notifications.Notify(this, Event.OnMouseMotionInput, inputEventMouseMotion);

			if (@event is InputEventJoypadButton inputEventJoypadButton)
				Notifications.Notify(this, Event.OnJoypadButtonInput, inputEventJoypadButton);

			if (Input.IsActionJustPressed("ui_fullscreen"))
				Options.ToggleFullscreen();

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
			MenuParticles.Position = new Vector2(OS.WindowSize.x / 2, OS.WindowSize.y);
			MenuParticles.ProcessMaterial.SetIndexed("emission_box_extents:x", OS.WindowSize.x / 2);
		}

		private async Task Cleanup()
		{
			if (Logger.StillWorking()) 
				await Task.Delay(1);
			
			ModLoader.SaveEnabled();
			Options.SaveOptions();
			await Net.Cleanup();
			Tokens.Cleanup();
			GetTree().Quit();
		}
	}
}
