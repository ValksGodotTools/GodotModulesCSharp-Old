namespace GodotModules;

public class Player : OtherPlayer
{
    [Export] protected readonly NodePath NodePathCamera;
    [Export] protected readonly NodePath NodePathAnimatedSprite;
    [Export] protected readonly NodePath NodePathSword;
    [Export] protected readonly NodePath NodePathSwordAnimationPlayer;
    [Export] protected readonly NodePath NodePathSwordSlashSprite;

    private Camera2D _camera;
    private AnimatedSprite _animatedSprite;
    private Node2D _sword;
    private Sprite _swordSlashSprite;
    private AnimationPlayer _swordAnimationPlayer;

    private bool _movingDown, _movingUp, _movingLeft, _movingRight, _running, _attack;
    private float _zoom = 0.75f;

    public int Gold;

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>(NodePathCamera);
        _animatedSprite = GetNode<AnimatedSprite>(NodePathAnimatedSprite);
        _sword = GetNode<Node2D>(NodePathSword);
        _swordAnimationPlayer = GetNode<AnimationPlayer>(NodePathSwordAnimationPlayer);
        _swordSlashSprite = GetNode<Sprite>(NodePathSwordSlashSprite);
        _swordSlashSprite.Visible = false;
        //_sprite = GetNode<Sprite>(NodePathSprite);

        Notifications.AddListener(this, Event.OnMouseButtonInput, OnMouseButtonInput);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("player_shoot") && !_swordAnimationPlayer.IsPlaying())
            _swordAnimationPlayer.Play("attack");
    }

    public override void _PhysicsProcess(float delta)
    {
        var mouseDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
        var newScale = _sword.Scale;

        if (_sword.Scale.y == 1 && mouseDirection.x < 0)
            newScale.y = -1;
        else if (_sword.Scale.y == -1 && mouseDirection.x > 0)
            newScale.y = 1;

        if (!_swordAnimationPlayer.IsPlaying())
        {
            _sword.Rotation = Mathf.LerpAngle(_sword.Rotation, mouseDirection.Angle(), 0.2f);
            _sword.Scale = newScale;
        }

        //_sprite.LerpRotationToTarget(GetGlobalMousePosition());
        HandleMovement(delta);
        HandleShoot();

        _camera.Zoom = _camera.Zoom.Lerp(new Vector2(_zoom, _zoom), 0.1f);
    }

    private void OnMouseButtonInput(Node sender, object[] args)
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

        if (area.IsInGroup("Coin"))
        {
            var coin = (Coin)area.GetParent();
            coin.MoveToPlayer(this);
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
        if (_movingUp || _movingDown || _movingLeft || _movingRight)
        {
            var mode = _running ? "run" : "walk";

            var dir =
                _movingUp ? "up" :
                _movingDown ? "down" :
                _movingLeft ? "left" :
                _movingRight ? "right" : throw new InvalidOperationException();

            _animatedSprite.Play($"{mode}_{dir}");
        }
        else
        {
            _animatedSprite.Play("idle");
        }

        if (_attack)
        {
            _attack = false;
            _animatedSprite.Play("attack");
        }
    }
}
