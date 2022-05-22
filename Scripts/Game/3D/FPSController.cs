using Godot;

namespace GodotModules
{
    public class FPSController : KinematicBody
    {
        [Export] protected readonly NodePath NodePathHead;
        [Export] protected readonly NodePath NodePathGroundCheck;
        [Export] protected readonly NodePath NodePathAnimationTree;
        [Export] protected readonly NodePath NodePathSkeleton;

        private Spatial _head;
        private RayCast _groundCheck;
        private AnimationTree _animationTree;
        private Skeleton _skeleton;
        private BoneAttachment _boneAttachment;

        private float _horzAcceleration = 15;
        private float _airAcceleration = 5;
        private float _normalAcceleration = 15;
        private float _mouseSensitivity = 0.10f;
        private float _moveSpeed = 5f;
        private float _gravityForce = 40f;
        private float _jumpForce = 10f;
        private int _jumpDelay = 500;
        private bool _canJump = true;
        private float _minAngleLookDown = -40f;
        private float _maxAngleLookUp = 80f;
        private bool _isWalking;

        private Vector3 _horzVelocity;
        private Vector3 _movement;
        private Vector3 _gravity;
        private Vector2 _mouseRelative;
        private bool _mouseMovement;

        private GTimer _jumpDelayTimer;

        public override void _Ready()
        {
            _head = GetNode<Spatial>(NodePathHead);
            _groundCheck = GetNode<RayCast>(NodePathGroundCheck);
            _animationTree = GetNode<AnimationTree>(NodePathAnimationTree);
            _skeleton = GetNode<Skeleton>(NodePathSkeleton);
            _boneAttachment = new BoneAttachment();
            _skeleton.AddChild(_boneAttachment);
            _boneAttachment.BoneName = "mixamorig_Head";

            _jumpDelayTimer = new GTimer(this, nameof(JumpDelayTimerFinished), _jumpDelay, false, false);

            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public override void _Process(float delta)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                var fullContact = _groundCheck.IsColliding();

                var headTransform = _head.GlobalTransform;
                headTransform.origin = _boneAttachment.GlobalTransform.origin;

                var mouseVelocity = _mouseMovement ? _mouseRelative : Vector2.Zero;

                RotateY((-mouseVelocity.x * _mouseSensitivity).ToRadians());
                _head.RotateX((-mouseVelocity.y * _mouseSensitivity).ToRadians());
                var headRotation = _head.Rotation;
                headRotation.x = Mathf.Clamp(headRotation.x, _minAngleLookDown.ToRadians(), _maxAngleLookUp.ToRadians());
                _head.Rotation = headRotation;

                _head.GlobalTransform = headTransform;
                _mouseMovement = false;
                
                HandleGravity(delta, fullContact);
                HandleJump(fullContact);
                HandleMovement(delta);

                _animationTree.Set("parameters/IdleWalkRun/blend_position", _horzVelocity.Length());
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            MoveAndSlide(_movement, Vector3.Up);
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                if (@event is InputEventMouseMotion motion)
                {
                    _mouseRelative = motion.Relative;
                    _mouseMovement = true;
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
                _animationTree.Set("parameters/JumpShot/active", true);
                //_gravity = Vector3.Up * _jumpForce;
                _jumpDelayTimer.Start();
                _canJump = false;
            }
        }

        private void HandleMovement(float delta)
        {
            var direction = new Vector3();

            var moveUp = Input.IsActionPressed("player_move_up");
            var moveDown = Input.IsActionPressed("player_move_down");
            var moveLeft = Input.IsActionPressed("player_move_left");
            var moveRight = Input.IsActionPressed("player_move_right");

            if (moveUp)
                direction -= Transform.basis.z;
            if (moveDown)
                direction += Transform.basis.z;
            if (moveLeft)
                direction -= Transform.basis.x;
            if (moveRight)
                direction += Transform.basis.x;

            if (moveUp || moveDown || moveLeft || moveRight)
            {
                _isWalking = true;
            }
            else
            {
                _isWalking = false;
            }

            var sprint = Input.IsActionPressed("player_sprint") ? 1.5f : 1f;

            direction = direction.Normalized();
            _horzVelocity = _horzVelocity.MoveToward(direction * _moveSpeed * sprint, _horzAcceleration * delta);
            _movement.z = _horzVelocity.z + _gravity.z;
            _movement.x = _horzVelocity.x + _gravity.x;
            _movement.y = _gravity.y;
        }

        private void JumpDelayTimerFinished() => _canJump = true;
    }
}
