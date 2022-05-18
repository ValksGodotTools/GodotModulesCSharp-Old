using Godot;

public class Chest : Node
{
    [Export] public readonly NodePath NodePathAnimatedSprite;

    private AnimatedSprite _animatedSprite;
    private bool _opened;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>(NodePathAnimatedSprite);
    }

    public void Open()
    {
        if (_opened)
            return;

        _opened = true;
        _animatedSprite.Play();
    }
}
