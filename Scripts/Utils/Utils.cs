using Godot;
using System;

using System.Diagnostics;

namespace GodotModules
{
    public static class Utils
    {
        public static float DegreesToRadians(float degrees) => degrees * (Mathf.Pi / 180);

        /// <summary>
        /// Lerp to an angle
        /// </summary>
        /// <param name="from">The start rotation in radians</param>
        /// <param name="to">The target rotation in degrees</param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static float LerpAngle(float from, float to, float weight) 
        {
            return from + ShortAngleDistance(from, DegreesToRadians(to)) * weight;
        }

        private static float ShortAngleDistance(float from, float to) 
        {
            var max_angle = Mathf.Pi * 2;
            var difference = (to - from) % (max_angle);
            return ((2 * difference) % (max_angle)) - difference;
        }

        public static void EscapeToScene(string scene, Action action)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                action();
                SceneManager.ChangeScene(scene);
            }
        }

        public static void LogErr(object obj)
        {
            ErrorNotifier.IncrementErrorCount();
            Log(obj, ConsoleColor.Red);
        }

        public static void Log(object obj, ConsoleColor color = ConsoleColor.Gray)
        {
            if (obj is Exception)
                UIDebugger.AddException((Exception)obj);
            else
                UIDebugger.AddMessage(obj);

            Console.ForegroundColor = color;
            GD.Print(obj);
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