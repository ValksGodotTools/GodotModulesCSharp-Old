using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GodotModules
{
    public static class Logger
    {
        public static void LogErr(Exception ex, string hint = "")
        {
            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogError, new GodotError
            {
                Exception = ex,
                Hint = hint
            }));
        }
        public static void LogTODO(object v, ConsoleColor color = ConsoleColor.White) => Log($"[TODO]: {v}", color);
        public static void LogWarning(object v, ConsoleColor color = ConsoleColor.Yellow) => Log($"[Warning]: {v}", color);
        public static void LogDebug(object v, ConsoleColor color = ConsoleColor.Magenta, bool trace = true, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) 
        {
            var path = "";
            if (trace)
                path = $"{filePath.Substring(filePath.IndexOf("Scripts\\"))} line:{lineNumber}";

            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, new GodotMessage
            {
                Text = $"[Debug]: {v}",
                Path = path,
                Color = color
            }));
        }
        public static void Log(object v, ConsoleColor color = ConsoleColor.Gray)
        {
            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, new GodotMessage
            {
                Text = $"{v}",
                Color = color
            }));
        }

        public static void LogMs(Action code)
        {
            var watch = new Stopwatch();
            watch.Start();
            code();
            watch.Stop();
            Log($"Took {watch.ElapsedMilliseconds} ms");
        }
    }
}