global using GodotModules;
global using GodotModules.Netcode;
global using System;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Runtime.CompilerServices;
global using System.Threading.Tasks;
global using System.Linq;

using Godot;

namespace GodotModules
{
    public class GM : Node
    {
        public static GM Instance { get; set; }
        public static NetworkManager Net = new NetworkManager();

        private const string GAME_NAME = "Godot Modules";
        private static Logger _logger;
        private SystemFileManager _systemFileManager;
        private static GodotCommands _godotCmds;
        private static SceneManager _sceneManager;
        private static GodotFileManager _godotFileManager;
        private static OptionsManager _optionsManager;

        public override async void _Ready()
        {
            Instance = this;

            _logger = new();
            _systemFileManager = new(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), GAME_NAME));
            _godotFileManager = new();
            _godotCmds = new();
            _sceneManager = new();
            _optionsManager = new(_systemFileManager);
            
            await _sceneManager.InitAsync();
            Net.StartServer(25565, 100);
            Net.StartClient("127.0.0.1", 25565);
            await Net.WebClient.CheckConnectionAsync();
        }

        public override async void _Process(float delta)
        {
            _logger.Update();
            await _godotCmds.Update();
        }

        public override void _Input(InputEvent @event)
        {
            _sceneManager.IfEscapePressed(async () => {
                await _sceneManager.ChangeScene(_sceneManager.PrevSceneName);
            });
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GetTree().SetAutoAcceptQuit(false);
                await Cleanup();
            }
        }

        public static Dictionary<string, JsonInputKey> Hotkeys => _optionsManager.Hotkeys;

        public static void SetHotkey(string action, InputEventKey inputEventKey) => _optionsManager.SetHotkey(action, inputEventKey);

        public static void Log(object v, ConsoleColor c = ConsoleColor.Gray) => _logger.Log(v, c);
        public static void LogWarning(object v, ConsoleColor c = ConsoleColor.Yellow) => _logger.LogWarning(v, c);
        public static void LogDebug(object v, bool trace = true, ConsoleColor c = ConsoleColor.Magenta, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => _logger.LogDebug(v, c, trace, filePath, lineNumber);
        public static void LogErr(Exception e, string hint = "", ConsoleColor c = ConsoleColor.Red) => _logger.LogErr(e, c, hint);
        public static void LogTodo(object v, ConsoleColor c = ConsoleColor.White) => _logger.LogTodo(v, c);
        public static void LogMs(Action a) => _logger.LogMs(a);

        public static void GodotCmd(GodotOpcode opcode, object v = null) => _godotCmds.Enqueue(opcode, v);

        public static async Task ChangeScene(string name, bool instant = true) => await _sceneManager.ChangeScene(name, instant);

        public static bool LoadDirectory(string path, Action<Directory, string> action) => _godotFileManager.LoadDir(path, action);

        public static void Quit() => Instance.Notification(MainLoop.NotificationWmQuitRequest);

        private async Task Cleanup()
        {
            _optionsManager.SaveHotkeys();
            await Net.Cleanup();
            GetTree().Quit();
        }
    }
}
