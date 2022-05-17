using Godot;

public class Enemy : KinematicBody2D
{
    [Export] public readonly NodePath NodePathSprite;
    private Sprite Sprite { get; set; }
    private Line2D _line2D;

    private List<OtherPlayer> _players;
    private Navigation2D _nav;
    private Vector2[] _path;
    private Vector2 _velocity;

    public void Init(List<OtherPlayer> players, Navigation2D nav, Line2D line2D)
    {
        _players = players;
        _nav = nav;
        _line2D = line2D;
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
            //Sprite.LookAt(target.Position);

            _path = _nav.GetSimplePath(GlobalPosition, target.GlobalPosition, false);
            _line2D.Points = _path;
            Navigate(delta);

            var dir = (target.Position - Position).Normalized();
            MoveAndSlide(_velocity);
        }
    }

    private void Navigate(float delta)
    {
        if (_path.Length > 0) 
        {
            var speed = 250f * 50;
            Sprite.LookAt(_path[1]);
            _velocity = GlobalPosition.DirectionTo(_path[1]) * delta * speed;
        }
    }
}
