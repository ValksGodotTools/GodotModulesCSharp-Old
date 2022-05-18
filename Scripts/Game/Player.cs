using Godot;

namespace GodotModules
{
    public class Player : OtherPlayer
    {
        [Export] public readonly NodePath NodePathCamera;
        [Export] public readonly NodePath NodePathAnimatedSprite;
        private Camera2D _camera;
        private AnimatedSprite _animatedSprite;

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
        }

        private void _on_Area2D_area_entered(Area2D area)
        {
            if (area.IsInGroup("Chest"))
            {
                var chest = (Chest)area.GetParent();
                chest.Open();
            }
        }

        private void HandleShoot()
        {
            if (Input.IsActionPressed("player_shoot"))
            {
                var shake = (ScreenShake)_camera.GetChild(0);
                shake.Start();
            }
        }

        private void HandleMovement(float delta)
        {
            var dir = new Vector2();

            var movingUp = Input.IsActionPressed("player_move_up");
            var movingDown = Input.IsActionPressed("player_move_down");
            var movingLeft = Input.IsActionPressed("player_move_left");
            var movingRight = Input.IsActionPressed("player_move_right");
            var running = Input.IsActionPressed("player_sprint");

            if (movingUp) dir.y -= 1;
            if (movingDown) dir.y += 1;
            if (movingLeft) dir.x -= 1;
            if (movingRight) dir.x += 1;

            HandleAnimation(movingUp, movingDown, movingLeft, movingRight, running);

            var speed = 250f * (running ? 2 : 1);

            MoveAndSlide(dir.Normalized() * speed * delta * 50);
        }

        private void HandleAnimation(bool movingUp, bool movingDown, bool movingLeft, bool movingRight, bool running)
        {
            if (!running)
            {
                if (movingUp)
                    _animatedSprite.Play("walk_up");
                else if (movingDown)
                    _animatedSprite.Play("walk_down");
                else if (movingLeft)
                    _animatedSprite.Play("walk_left");
                else if (movingRight)
                    _animatedSprite.Play("walk_right");
            }

            if (running)
            {
                if (movingUp)
                    _animatedSprite.Play("run_up");
                else if (movingDown)
                    _animatedSprite.Play("run_down");
                else if (movingLeft)
                    _animatedSprite.Play("run_left");
                else if (movingRight)
                    _animatedSprite.Play("run_right");
            }

            if (!movingUp && !movingDown && !movingLeft && !movingRight)
                _animatedSprite.Play("idle");
        }
    }
}
