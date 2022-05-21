using Godot;

namespace GodotModules
{
    public class FPSController : KinematicBody
    {
        [Export] protected readonly NodePath NodePathHead;
        [Export] protected readonly NodePath NodePathGroundCheck;

        private Spatial _head;
        private RayCast _groundCheck;

        private float _horzAcceleration = 50;
        private float _airAcceleration = 20;
        private float _normalAcceleration = 50;
        private float _mouseSensitivity = 0.10f;
        private float _moveSpeed = 10f;
        private float _gravityForce = 40f;
        private float _jumpForce = 10f;
        private int _jumpDelay = 500;
        private bool _canJump = true;

        private Vector3 _horzVelocity;
        private Vector3 _movement;
        private Vector3 _gravity;

        private GTimer _jumpDelayTimer;

        public override void _Ready()
        {
            _head = GetNode<Spatial>(NodePathHead);
            _groundCheck = GetNode<RayCast>(NodePathGroundCheck);

            _jumpDelayTimer = new GTimer(this, nameof(JumpDelayTimerFinished), _jumpDelay, false, false);

            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public override void _PhysicsProcess(float delta)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                var fullContact = _groundCheck.IsColliding();

                HandleGravity(delta, fullContact);
                HandleJump(fullContact);
                HandleMovement(delta);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                if (@event is InputEventMouseMotion motion) 
                {
                    RotateY((-motion.Relative.x * _mouseSensitivity).ToRadians());
                    _head.RotateX((-motion.Relative.y * _mouseSensitivity).ToRadians());
                    var headRotation = _head.Rotation;
                    headRotation.x = Mathf.Clamp(headRotation.x, -89f.ToRadians(), 89f.ToRadians());
                    _head.Rotation = headRotation;
                }
            }

            if (Input.GetMouseMode() == Input.MouseMode.Visible)
                if (@event is InputEventMouseButton button)
                    if (button.ButtonIndex == (int)ButtonList.Left)
                        Input.SetMouseMode(Input.MouseMode.Captured);

            if (Input.IsActionJustPressed("ui_cancel"))
                Input.SetMouseMode(Input.MouseMode.Visible);
        }

        private void HandleGravity(float delta, bool fullContact)
        {
            if (!IsOnFloor()) 
            {
                _gravity += Vector3.Down * _gravityForce * delta;
                _horzAcceleration = _airAcceleration;
            }
            else if (IsOnFloor() && fullContact) 
            {
                _gravity = -GetFloorNormal() * _gravityForce;
                _horzAcceleration = _normalAcceleration;
            }
            else 
            {
                _gravity = -GetFloorNormal();
                _horzAcceleration = _normalAcceleration;
            }
        }

        private void HandleJump(bool fullContact)
        {
            if (Input.IsActionJustPressed("player_jump") && _canJump && (IsOnFloor() || fullContact)) 
            {
                _gravity = Vector3.Up * _jumpForce;
                _jumpDelayTimer.Start();
                _canJump = false;
            }
        }

        private void HandleMovement(float delta)
        {
            var direction = new Vector3();

            if (Input.IsActionPressed("player_move_up"))
                direction -= Transform.basis.z;
            if (Input.IsActionPressed("player_move_down"))
                direction += Transform.basis.z;
            if (Input.IsActionPressed("player_move_left"))
                direction -= Transform.basis.x;
            if (Input.IsActionPressed("player_move_right"))
                direction += Transform.basis.x;

            direction = direction.Normalized();
            _horzVelocity = _horzVelocity.MoveToward(direction * _moveSpeed, _horzAcceleration * delta);
            _movement.z = _horzVelocity.z + _gravity.z;
            _movement.x = _horzVelocity.x + _gravity.x;
            _movement.y = _gravity.y;

            MoveAndSlide(_movement, Vector3.Up);
        }

        private void JumpDelayTimerFinished() => _canJump = true;
    }
}
