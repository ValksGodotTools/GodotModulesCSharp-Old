using Godot;

namespace GodotModules
{
    public class Coin : RigidBody2D
    {
        [Export] protected readonly NodePath NodePathAnimatedSprite;

        public override void _Ready()
        {
            var animatedSprite = GetNode<AnimatedSprite>(NodePathAnimatedSprite);
            animatedSprite.Frame = (int)GD.RandRange(0, animatedSprite.Frames.GetFrameCount("Coin"));
            animatedSprite.Playing = true;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            RotationDegrees = 0;
        }
    }
}
