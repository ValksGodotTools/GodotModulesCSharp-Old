using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GodotModules
{
    public static class Utils
    {
        public static float DegreesToRadians(float degrees) => degrees * (Mathf.Pi / 180);

        public static float LerpAngle(float from, float to, float weight)
        {
            return from + ShortAngleDistance(from, to) * weight;
        }

        private static float ShortAngleDistance(float from, float to)
        {
            var max_angle = Mathf.Pi * 2;
            var difference = (to - from) % (max_angle);
            return ((2 * difference) % (max_angle)) - difference;
        }

        public static void LogErr(Exception ex, string hint = "")
        {
            ErrorNotifier.IncrementErrorCount();

            var message = $"[Error]: {hint}{ex.Message}\n{ex.StackTrace}";
            UIDebugger.AddMessage(message);

            Colorize(() => GD.PrintErr(message), ConsoleColor.Red);
        }

        public static void LogWarning(string obj, ConsoleColor color = ConsoleColor.Yellow) 
        {
            var message = $"[Warning]: {obj}";
            UIDebugger.AddMessage(message);
            Colorize(() => GD.Print(message), color);
        }

        public static void LogDebug(string obj, ConsoleColor color = ConsoleColor.Magenta) 
        {
            var message = $"[Debug]: {obj}";
            UIDebugger.AddMessage(message);
            Colorize(() => GD.Print(message), color);
        }

        public static void Log(string obj, ConsoleColor color = ConsoleColor.Gray)
        {
            UIDebugger.AddMessage(obj);
            Colorize(() => GD.Print(obj), color);
        }

        private static void Colorize(Action action, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            action();
            Console.ResetColor();
        }

        public static void LogMs(Action code)
        {
            var watch = new Stopwatch();
            watch.Start();
            code();
            watch.Stop();
            Console.WriteLine($"Took {watch.ElapsedMilliseconds} ms");
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new Vector2(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t));

        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }
    }
}