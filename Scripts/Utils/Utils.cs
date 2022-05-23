using Godot;

namespace GodotModules
{
    public static class Utils
    {
        public static Vector2 RandomDir() 
        {
            float angle = GD.Randf() * Mathf.Pi * 2;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => 
            new Vector2(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t));

        public static List<T> GetEnumList<T>() =>
            new List<T>((T[])Enum.GetValues(typeof(T)));
    }
}