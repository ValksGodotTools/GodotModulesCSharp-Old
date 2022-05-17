using Godot;

public class Enemy : KinematicBody2D
{
    [Export] public readonly NodePath NodePathSprite;
    private Sprite Sprite { get; set; }

    private List<OtherPlayer> _players;

    public void Init(List<OtherPlayer> players)
    {
        _players = players;
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
            Sprite.LookAt(target.Position);

            var dir = (target.Position - Position).Normalized();
            var speed = 250f;
            MoveAndSlide(dir * speed * delta * 50);
        }
    }
}
