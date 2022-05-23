using Godot;

namespace GodotModules
{
    public class Chest : Node2D
    {
        [Export] protected readonly NodePath NodePathAnimatedSprite;

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

        private void _on_AnimatedSprite_animation_finished()
        {
            //var orb = Prefabs.Orb.Instance<RigidBody2D>();
            //orb.Position = Position + new Vector2(0, 100);
            Logger.Log(GetTree().Root.Name);
        }
    }
}
