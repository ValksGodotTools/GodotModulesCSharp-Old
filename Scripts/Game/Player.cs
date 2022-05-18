using Godot;

public class Player : OtherPlayer
{
    [Export] public readonly NodePath NodePathCamera;
    private Camera2D _camera;

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>(NodePathCamera);
        _sprite = GetNode<Sprite>(NodePathSprite);
    }

    public override void _PhysicsProcess(float delta)
    {
        _sprite.LerpRotationToTarget(GetGlobalMousePosition());
        HandleMovement(delta);
        HandleShoot();
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

        if (Input.IsActionPressed("player_move_up"))
            dir.y -= 1;
        if (Input.IsActionPressed("player_move_down"))
            dir.y += 1;
        if (Input.IsActionPressed("player_move_left"))
            dir.x -= 1;
        if (Input.IsActionPressed("player_move_right"))
            dir.x += 1;

        var Speed = 250f;
        MoveAndSlide(dir * Speed * delta * 50);
    }
}
