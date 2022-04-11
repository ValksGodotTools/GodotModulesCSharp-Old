using Godot;
using System;

namespace Valk.Modules
{
    public static class Utils
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float by)
        {
            float retX = Mathf.Lerp(a.x, b.x, by);
            float retY = Mathf.Lerp(a.y, b.y, by);
            return new Vector2(retX, retY);
        }
    }
}
