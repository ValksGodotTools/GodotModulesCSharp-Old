namespace GodotModules
{
    public class Chest : Node2D
    {
        [Export] protected readonly NodePath NodePathAnimatedSprite;

        private AnimatedSprite _animatedSprite;
        private bool _opened;
        private SceneGame _game;

        public void PreInit(SceneGame game) => _game = game;

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
            for (int i = 0; i < 10; i++)
            {
                var coin = Prefabs.Coin.Instance<Coin>();
                coin.GlobalPosition = GlobalPosition + new Vector2(0, 50) + (Utils.RandomDir() * 25) * GD.Randf();
                _game.CoinList.AddChild(coin);
            }
        }
    }
}
