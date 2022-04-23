using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GodotModules
{
    public static class Utils
    {
        public static void EscapeToScene(string scene, Action action)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                action();
                SceneManager.ChangeScene(scene);
            }
        }

        public static void Log(object obj, ConsoleColor color = ConsoleColor.Gray) 
        {
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
            System.Console.WriteLine($"Took {watch.ElapsedMilliseconds} ms");
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