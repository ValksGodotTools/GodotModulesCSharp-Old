using Godot;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules
{
    public class GameManager : Node
    {
        [Export] public readonly NodePath NodePathGameConsole;
        public static ModLoader ModLoader { get; set; }
        public static UIGameConsole GameConsole { get; set; }
        public static GodotCommands GodotCommands { get; set; }
        public static Logger Logger { get; set; }

        public static string GameName = "Godot Modules";
        public static OptionsData Options { get; set; }
        public static SceneTree GameTree { get; set; }

        public override void _Ready()
        {
            GameTree = GetTree();
            GameConsole = GetNode<UIGameConsole>(NodePathGameConsole);
            GodotCommands = new GodotCommands();
            Logger = new Logger();
        }

        public override async void _Process(float delta)
        {
            Logger.Dequeue();
            NetworkManager.ServerSimulation.Dequeue();
            await GodotCommands.Dequeue();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_debug"))
            {
                GameConsole.ToggleVisibility();
            }

            if (Input.IsActionJustPressed("ui_fullscreen"))
                UtilOptions.ToggleFullscreen();
        }

        public static string GetGameDataPath() => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), GameManager.GameName);

        public static void SpawnPopupMessage(string message)
        {
            var popupMessage = Prefabs.PopupMessage.Instance<UIPopupMessage>();
            GameTree.CurrentScene.AddChild(popupMessage);
            popupMessage.Init(message);
            popupMessage.PopupCentered();
        }

        public static void SpawnPopupError(Exception e)
        {
            ErrorNotifier.IncrementErrorCount();
            var popupError = Prefabs.PopupError.Instance<UIPopupError>();
            GameTree.CurrentScene.AddChild(popupError);
            popupError.Init(e.Message, e.StackTrace);
            popupError.PopupCentered();
        }

        /// <summary>
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => GameTree.Notification(MainLoop.NotificationWmQuitRequest);
    }
}