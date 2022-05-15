using Godot;

namespace GodotModules
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        public static float ToRadians(this float degrees) => degrees * (Mathf.Pi / 180);
        public static float LerpAngle(this float from, float to, float weight) => from + ShortAngleDistance(from, to) * weight;
        private static float ShortAngleDistance(float from, float to)
        {
            var max_angle = Mathf.Pi * 2;
            var difference = (to - from) % (max_angle);
            return ((2 * difference) % (max_angle)) - difference;
        }
    }
}