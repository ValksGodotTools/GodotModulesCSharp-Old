using Godot;

public class Enemy : KinematicBody2D
{
    [Export] public readonly NodePath NodePathSprite;
    private Sprite Sprite { get; set; }

    private List<OtherPlayer> _players;
    private Navigation2D _nav;
    private Vector2[] _path;
    private Vector2 _velocity;

    public void Init(List<OtherPlayer> players, Navigation2D nav)
    {
        _players = players;
        _nav = nav;
    }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>(NodePathSprite);
    }

    public override void _PhysicsProcess(float delta)
    {
        var target = _players.FirstOrDefault();

        if (target != null) 
        {
            _path = _nav.GetSimplePath(GlobalPosition, target.GlobalPosition, false);
            Navigate(delta);

            var dir = (target.Position - Position).Normalized();
            MoveAndSlide(_velocity);
        }
    }

    private void Navigate(float delta)
    {
        if (_path.Length > 0) 
        {
            var speed = 150f * 50;
            Sprite.LerpRotationToTarget(_path[1]);
            //Sprite.Rotation = Mathf.LerpAngle(Sprite.Rotation, (_path[1] - Position).Angle(), 0.1f);
            _velocity = GlobalPosition.DirectionTo(_path[1]) * delta * speed;
        }
    }
}
