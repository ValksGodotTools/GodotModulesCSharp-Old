using Godot;

namespace GodotModules
{
    public class Coin : RigidBody2D
    {
        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            RotationDegrees = 0;
        }
    }
}
