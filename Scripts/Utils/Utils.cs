

namespace GodotModules
{
    public static class Utils
    {
        public static Vector2 RandomDir() 
        {
            var angle = GD.Randf() * Mathf.Pi * 2;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => 
            new(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t));

        public static List<T> GetEnumList<T>() =>
            new((T[])Enum.GetValues(typeof(T)));
    }
}