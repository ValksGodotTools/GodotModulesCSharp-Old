using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GodotModules
{
    public class Logger
    {
        private ConcurrentQueue<ThreadCmd<LoggerOpcode>> Messages = new ConcurrentQueue<ThreadCmd<LoggerOpcode>>();

        public void LogErr(Exception ex, string hint = "")
        {
            Messages.Enqueue(new ThreadCmd<LoggerOpcode>(LoggerOpcode.LogError, new GodotError
            {
                Exception = ex,
                Hint = hint
            }));
        }
        public void LogTODO(object v, ConsoleColor color = ConsoleColor.White) => Log($"[TODO]: {v}", color);
        public void LogWarning(object v, ConsoleColor color = ConsoleColor.Yellow) => Log($"[Warning]: {v}", color);
        public void LogDebug(object v, ConsoleColor color = ConsoleColor.Magenta, bool trace = true, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var path = "";
            if (trace)
                path = $"{filePath.Substring(filePath.IndexOf("Scripts\\"))} line:{lineNumber}";

            Messages.Enqueue(new ThreadCmd<LoggerOpcode>(LoggerOpcode.LogMessage, new GodotMessage
            {
                Text = $"[Debug]: {v}",
                Path = path,
                Color = color
            }));
        }
        public void Log(object v, ConsoleColor color = ConsoleColor.Gray)
        {
            Messages.Enqueue(new ThreadCmd<LoggerOpcode>(LoggerOpcode.LogMessage, new GodotMessage
            {
                Text = $"{v}",
                Color = color
            }));
        }

        public void LogMs(Action code)
        {
            var watch = new Stopwatch();
            watch.Start();
            code();
            watch.Stop();
            Log($"Took {watch.ElapsedMilliseconds} ms");
        }

        public void Dequeue()
        {
            if (Messages.TryDequeue(out ThreadCmd<LoggerOpcode> cmd))
            {
                var opcode = cmd.Opcode;

                switch (opcode)
                {
                    case LoggerOpcode.LogMessage:
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
                        GM.LogConsoleMessage(text);
#endif
                        break;
                    case LoggerOpcode.LogError:
                        var error = (GodotError)cmd.Data;
                        var exception = error.Exception;
                        var hint = error.Hint;

                        var errorText = $"[Error]: {hint}{exception.Message}\n{exception.StackTrace}";

                        Console.ForegroundColor = ConsoleColor.Red;
                        GD.PrintErr(exception);
                        Console.ResetColor();

#if CLIENT
                        ErrorNotifier.IncrementErrorCount();
                        GM.LogConsoleMessage(errorText);
#endif
                        break;
                }
            }
        }
    }
}