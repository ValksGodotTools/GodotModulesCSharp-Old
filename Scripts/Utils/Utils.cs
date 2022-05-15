using Godot;
using System;

namespace GodotModules
{
    public static class Utils
    {
        public static Vector2 RandomDir() 
        {
            float angle = GD.Randf();
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static float DegreesToRadians(float degrees) => degrees * (Mathf.Pi / 180);

        public static float LerpAngle(float from, float to, float weight) => from + ShortAngleDistance(from, to) * weight;

        private static float ShortAngleDistance(float from, float to)
        {
            var max_angle = Mathf.Pi * 2;
            var difference = (to - from) % (max_angle);
            return ((2 * difference) % (max_angle)) - difference;
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