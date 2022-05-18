using Godot;

namespace GodotModules
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2) =>
            (value - from1) / (to1 - from1) * (to2 - from2) + from2;

        public static void LerpRotationToTarget(this Sprite sprite, Vector2 target, float t = 0.1f) =>
            sprite.Rotation = Mathf.LerpAngle(sprite.Rotation, (target - sprite.GlobalPosition).Angle(), t);

        public static float ToRadians(this float degrees) =>
            degrees * (Mathf.Pi / 180);
    }
}