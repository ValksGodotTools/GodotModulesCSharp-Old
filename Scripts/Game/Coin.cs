using Godot;

namespace GodotModules
{
    public class Coin : RigidBody2D
    {
        [Export] protected readonly NodePath NodePathAnimatedSprite;

        public OtherPlayer Target { get; set; }

        public override void _Ready()
        {
            var animatedSprite = GetNode<AnimatedSprite>(NodePathAnimatedSprite);
            animatedSprite.Frame = (int)GD.RandRange(0, animatedSprite.Frames.GetFrameCount("Coin"));
            animatedSprite.Playing = true;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            RotationDegrees = 0;

            if (Target != null)
            {
                var dir = (Target.Position - Position).Normalized();
                LinearVelocity = dir * 10;
            }
        }
    }
}
