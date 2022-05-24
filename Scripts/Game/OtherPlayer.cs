

namespace GodotModules
{
    public class OtherPlayer : KinematicBody2D
    {
        [Export] protected readonly NodePath NodePathSprite;
        protected Sprite _sprite;

        public override void _Ready()
        {
            _sprite = GetNode<Sprite>(NodePathSprite);
        }
    }
}
