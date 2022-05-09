using Godot;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GodotModules
{
    public static class Logger
    {
        private static ConcurrentQueue<GodotCmd> Messages = new ConcurrentQueue<GodotCmd>();

        public static void LogErr(Exception ex, string hint = "")
        {
            Messages.Enqueue(new GodotCmd(GodotOpcode.LogError, new GodotError
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

            Messages.Enqueue(new GodotCmd(GodotOpcode.LogMessage, new GodotMessage
            {
                Text = $"[Debug]: {v}",
                Path = path,
                Color = color
            }));
        }
        public static void Log(object v, ConsoleColor color = ConsoleColor.Gray)
        {
            Messages.Enqueue(new GodotCmd(GodotOpcode.LogMessage, new GodotMessage
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

        public static void Dequeue()
        {
            if (Messages.TryDequeue(out GodotCmd cmd))
            {
                var opcode = cmd.Opcode;

                switch (opcode)
                {
                    case GodotOpcode.LogMessage:
                        var message = (GodotMessage)cmd.Data;
                        var text = message.Text;
                        var color = message.Color;

                        Console.ForegroundColor = color;
                        GD.Print(text);

                        if (!string.IsNullOrWhiteSpace(message.Path))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            GD.Print($"   at ({message.Path})");
                        }

                        Console.ResetColor();

#if CLIENT
                        UIDebugger.AddMessage(text);
#endif
                        break;
                    case GodotOpcode.LogError:
                        var error = (GodotError)cmd.Data;
                        var exception = error.Exception;
                        var hint = error.Hint;

                        var errorText = $"[Error]: {hint}{exception.Message}\n{exception.StackTrace}";

                        Console.ForegroundColor = ConsoleColor.Red;
                        GD.PrintErr(exception);
                        Console.ResetColor();

#if CLIENT
                        ErrorNotifier.IncrementErrorCount();
                        UIDebugger.AddMessage(errorText);
#endif
                        break;
                }
            }
        }
    }
}