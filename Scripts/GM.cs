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

        private static Logger _logger = new Logger();
        private static GodotCommands _godotCmds = new GodotCommands();
        private static SceneManager _sceneManager = new SceneManager();
        private static GodotFileManager _godotFileManager = new GodotFileManager();

        public override async void _Ready()
        {
            Instance = this;
            await _sceneManager.Init();
            Net.StartServer(25565, 100);
            Net.StartClient("127.0.0.1", 25565);
        }

        public override void _Process(float delta)
        {
            _logger.Update();
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GetTree().SetAutoAcceptQuit(false);
                await Cleanup();
            }
        }

        public static void Log(object v, ConsoleColor c = ConsoleColor.Gray) => _logger.Log(v, c);
        public static void LogWarning(object v, ConsoleColor c = ConsoleColor.Yellow) => _logger.LogWarning(v, c);
        public static void LogDebug(object v, bool trace = true, ConsoleColor c = ConsoleColor.Magenta, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => _logger.LogDebug(v, c, trace, filePath, lineNumber);
        public static void LogErr(Exception e, string hint = "", ConsoleColor c = ConsoleColor.Red) => _logger.LogErr(e, c, hint);
        public static void LogTodo(object v, ConsoleColor c = ConsoleColor.White) => _logger.LogTodo(v, c);
        public static void LogMs(Action a) => _logger.LogMs(a);

        public static void GodotCmd(GodotOpcode opcode, object v = null) => _godotCmds.Enqueue(opcode, v);

        public static async Task ChangeScene(string name, bool instant = false) => await _sceneManager.ChangeScene(name, instant);

        public static bool LoadDirectory(string path, Action<Directory, string> action) => _godotFileManager.LoadDir(path, action);

        private async Task Cleanup()
        {
            await Net.Cleanup();
            GetTree().Quit();
        }
    }
}
