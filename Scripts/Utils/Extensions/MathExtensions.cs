namespace GodotModules
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}