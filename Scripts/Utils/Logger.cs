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
        public static void LogWarning(object v, ConsoleColor color = ConsoleColor.Yellow) => Log($"[Warning]: {v}", color);
        public static void LogDebug(object v, ConsoleColor color = ConsoleColor.Magenta) => Log($"[Debug]: {v}", color);
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