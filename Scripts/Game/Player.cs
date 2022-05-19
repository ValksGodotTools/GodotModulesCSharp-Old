using Godot;

namespace GodotModules
{
    public class Player : OtherPlayer
    {
        [Export] public readonly NodePath NodePathCamera;
        [Export] public readonly NodePath NodePathAnimatedSprite;
        private Camera2D _camera;
        private AnimatedSprite _animatedSprite;

        private bool _movingDown, _movingUp, _movingLeft, _movingRight, _running, _attack;
        private float _zoom = 0.75f;

        public override void _Ready()
        {
            _camera = GetNode<Camera2D>(NodePathCamera);
            _animatedSprite = GetNode<AnimatedSprite>(NodePathAnimatedSprite);
            //_sprite = GetNode<Sprite>(NodePathSprite);
        }

        public override void _PhysicsProcess(float delta)
        {
            //_sprite.LerpRotationToTarget(GetGlobalMousePosition());
            HandleMovement(delta);
            HandleShoot();

            _camera.Zoom = _camera.Zoom.Lerp(new Vector2(_zoom, _zoom), 0.1f);
        }

        public override void _Input(InputEvent @event)
        {
            HandleCameraZoom();
        }

        private void _on_Area2D_area_entered(Area2D area)
        {
            if (area.IsInGroup("Chest"))
            {
                var chest = (Chest)area.GetParent();
                chest.Open();
            }
        }

        private void HandleCameraZoom()
        {
            if (Input.IsActionPressed("camera_zoom_in"))
                _zoom -= 0.1f;

            if (Input.IsActionPressed("camera_zoom_out"))
                _zoom += 0.1f;

            _zoom = _zoom.Clamp(0.3f, 1.5f);
        }

        private void HandleShoot()
        {
            var shooting = Input.IsActionPressed("player_shoot");

            _attack = Input.IsActionJustPressed("player_shoot");

            if (shooting)
            {
                var shake = (ScreenShake)_camera.GetChild(0);
                shake.Start();
            }
        }

        private void HandleMovement(float delta)
        {
            var dir = new Vector2();

            _movingUp = Input.IsActionPressed("player_move_up");
            _movingDown = Input.IsActionPressed("player_move_down");
            _movingLeft = Input.IsActionPressed("player_move_left");
            _movingRight = Input.IsActionPressed("player_move_right");
            _running = Input.IsActionPressed("player_sprint");

            if (_movingUp) dir.y -= 1;
            if (_movingDown) dir.y += 1;
            if (_movingLeft) dir.x -= 1;
            if (_movingRight) dir.x += 1;

            HandleAnimation();

            var speed = 250f * (_running ? 2 : 1);

            MoveAndSlide(dir.Normalized() * speed * delta * 50);
        }

        private void HandleAnimation()
        {
            if (!_running)
            {
                if (_movingUp)
                    _animatedSprite.Play("walk_up");
                else if (_movingDown)
                    _animatedSprite.Play("walk_down");
                else if (_movingLeft)
                    _animatedSprite.Play("walk_left");
                else if (_movingRight)
                    _animatedSprite.Play("walk_right");
            }

            if (_running)
            {
                if (_movingUp)
                    _animatedSprite.Play("run_up");
                else if (_movingDown)
                    _animatedSprite.Play("run_down");
                else if (_movingLeft)
                    _animatedSprite.Play("run_left");
                else if (_movingRight)
                    _animatedSprite.Play("run_right");
            }

            if (!_movingUp && !_movingDown && !_movingLeft && !_movingRight)
                _animatedSprite.Play("idle");

            if (_attack) 
            {
                _attack = false;
                _animatedSprite.Play("attack");
            }
        }
    }
}
