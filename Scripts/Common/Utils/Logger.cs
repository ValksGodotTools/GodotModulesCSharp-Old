using Godot;

namespace GodotModules
{
    public class Logger
    {
        private ConcurrentQueue<LogInfo> _messages = new ConcurrentQueue<LogInfo>();

        public void LogErr(Exception e, ConsoleColor c, string hint) => _messages.Enqueue(new LogInfo(LoggerOpcode.Exception, $"[Error]: {(string.IsNullOrWhiteSpace(hint) ? "" : $"'{hint}' ")}{e.Message}\n{e.StackTrace}", c));
        public void LogDebug(object v, ConsoleColor c, bool trace, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) => _messages.Enqueue(new LogInfo(LoggerOpcode.Debug, new LogMessageDebug($"[Debug]: {v}", trace, $"   at {filePath.Substring(filePath.IndexOf("Scripts\\"))} line:{lineNumber}"), c));
        public void LogTodo(object v, ConsoleColor c) => Log($"[Todo]: {v}", c);
        public void LogWarning(object v, ConsoleColor c) => Log($"[Warning]: {v}", c);
        public void Log(object v, ConsoleColor c) => _messages.Enqueue(new LogInfo(LoggerOpcode.Message, $"{v}", c));
        public void LogMs(Action code)
        {
            var watch = new Stopwatch();
            watch.Start();
            code();
            watch.Stop();
            Log($"Took {watch.ElapsedMilliseconds} ms", ConsoleColor.DarkGray);
        }

        public void Update()
        {
            if (_messages.TryDequeue(out LogInfo result))
            {
                switch (result.Opcode)
                {
                    case LoggerOpcode.Message:
                        Print((string)result.Data, result.Color);
                        Console.ResetColor();
                        break;

                    case LoggerOpcode.Exception:
                        PrintErr((string)result.Data, result.Color);
                        Console.ResetColor();
                        break;

                    case LoggerOpcode.Debug:
                        var data = (LogMessageDebug)result.Data;
                        Print(data.Message, result.Color);
                        if (data.Trace)
                            Print(data.TracePath, ConsoleColor.DarkGray);
                        Console.ResetColor();
                        break;
                }
            }
        }

        private void Print(object v, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            GD.Print(v);
        }

        private void PrintErr(object v, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            GD.PrintErr(v);
        }
    }

    public class LogInfo
    {
        public LoggerOpcode Opcode { get; set; }
        public object Data { get; set; }
        public ConsoleColor Color { get; set; }

        public LogInfo(LoggerOpcode opcode, object data, ConsoleColor color = ConsoleColor.Gray) 
        {
            Opcode = opcode;
            Data = data;
            Color = color;
        }
    }

    public class LogMessageDebug 
    {
        public string Message { get; set; }
        public bool Trace { get; set; }
        public string TracePath { get; set; }

        public LogMessageDebug(string message, bool trace = true, string tracePath = "")
        {
            Message = message;
            Trace = trace;
            TracePath = tracePath;
        }
    }

    public enum LoggerOpcode 
    {
        Message,
        Exception,
        Debug
    }
}
