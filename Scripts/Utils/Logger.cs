using System;
using System.Diagnostics;

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
        public static void LogWarning(string text, ConsoleColor color = ConsoleColor.Yellow) => Log($"[Warning]: {text}", color);
        public static void LogDebug(string text, ConsoleColor color = ConsoleColor.Magenta) => Log($"[Debug]: {text}", color);
        public static void Log(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            NetworkManager.GodotCmds.Enqueue(new GodotCmd(GodotOpcode.LogMessage, new GodotMessage
            {
                Text = text,
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