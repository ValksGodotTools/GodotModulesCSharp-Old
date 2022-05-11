global using GodotModules;
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
        private static Logger _logger = new Logger();

        public override void _Ready()
        {

        }

        public override void _Process(float delta)
        {
            _logger.Update();
        }

        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GetTree().SetAutoAcceptQuit(false);
                GetTree().Quit();
            }
        }

        public static void Log(object v, ConsoleColor c = ConsoleColor.Gray) => _logger.Log(v, c);
        public static void LogWarning(object v, ConsoleColor c = ConsoleColor.Yellow) => _logger.LogWarning(v, c);
        public static void LogDebug(object v, bool trace = true, ConsoleColor c = ConsoleColor.Magenta, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => _logger.LogDebug(v, c, trace, filePath, lineNumber);
        public static void LogErr(Exception e, string hint = "", ConsoleColor c = ConsoleColor.Red) => _logger.LogErr(e, c, hint);
        public static void LogTodo(object v, ConsoleColor c = ConsoleColor.White) => _logger.LogTodo(v, c);
        public static void LogMs(Action a) => _logger.LogMs(a);
    }
}
