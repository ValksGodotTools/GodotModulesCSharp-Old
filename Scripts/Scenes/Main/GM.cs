using Godot;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System;
using System.Runtime.CompilerServices;

namespace GodotModules
{
    public class GM : Node
    {
        [Export] public readonly NodePath NodePathGameConsole;
        
        public static ModLoader ModLoader { get; set; }
        public static string GameName = "Godot Modules";
        public static OptionsData Options { get; set; }
        public static SceneTree PersistentTree { get; set; }

        private static UIGameConsole GameConsole { get; set; }
        private static GodotCommands GodotCommands { get; set; }
        private static Logger Logger { get; set; }

        public override void _Ready()
        {
            PersistentTree = GetTree();
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

        public static string GetGameDataPath() => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), GM.GameName);

        public static void SpawnPopupMessage(string message)
        {
            var popupMessage = Prefabs.PopupMessage.Instance<UIPopupMessage>();
            PersistentTree.CurrentScene.AddChild(popupMessage);
            popupMessage.Init(message);
            popupMessage.PopupCentered();
        }

        public static void SpawnPopupError(Exception e)
        {
            ErrorNotifier.IncrementErrorCount();
            var popupError = Prefabs.PopupError.Instance<UIPopupError>();
            PersistentTree.CurrentScene.AddChild(popupError);
            popupError.Init(e.Message, e.StackTrace);
            popupError.PopupCentered();
        }

        public static void LogConsoleMessage(string message) => GameConsole.AddMessage(message);
        public static void ToggleConsoleVisibility() => GameConsole.ToggleVisibility();
        public static bool GameConsoleVisible => GameConsole.Visible;

        public static void EnqueueGodotCmd(GodotOpcode opcode, object data = null) => GodotCommands.Enqueue(opcode, data);

        public static void Log(object v, ConsoleColor color = ConsoleColor.Gray) => Logger.Log(v, color);
        public static void LogWarning(object v, ConsoleColor color = ConsoleColor.Yellow) => Logger.LogWarning(v, color);
        public static void LogDebug(object v, ConsoleColor color = ConsoleColor.Magenta, bool trace = true, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => Logger.LogDebug(v, color, trace, filePath, lineNumber);
        public static void LogErr(Exception ex, string hint = "") => Logger.LogErr(ex, hint);
        public static void LogTODO(object v, ConsoleColor color = ConsoleColor.White) => Logger.LogTODO(v, color);
        public static void LogMs(Action code) => Logger.LogMs(code);

        public static void Exit() => PersistentTree.Notification(MainLoop.NotificationWmQuitRequest);
    }
}