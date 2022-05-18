using Godot;

namespace GodotModules
{
    public class FPSController : KinematicBody
    {
        private float _moveSpeed = 500f;
        private float _gravity = -200;

        public override void _PhysicsProcess(float delta)
        {
            var velocity = new Vector3();
            velocity = CalcMovement() + CalcGravity();

            MoveAndSlide(velocity * delta);
        }

        private Vector3 CalcGravity() => new Vector3(0, _gravity, 0);

        private Vector3 CalcMovement()
        {
            var dir = new Vector2();

            if (Input.IsActionPressed("player_move_up"))
                dir.y -= 1;
            if (Input.IsActionPressed("player_move_down"))
                dir.y += 1;
            if (Input.IsActionPressed("player_move_left"))
                dir.x -= 1;
            if (Input.IsActionPressed("player_move_right"))
                dir.x += 1;

            return new Vector3(dir.x, 0, dir.y) * _moveSpeed;
        }
    }
}
