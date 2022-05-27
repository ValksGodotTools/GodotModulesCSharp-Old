namespace GodotModules
{
    public class Coin : AnimatedSprite
    {
        private bool _movingToPlayer;
        private Player _player;

        public override void _Ready()
        {
            Frame = (int)GD.RandRange(0, Frames.GetFrameCount("default"));
            Playing = true;
        }

        public void MoveToPlayer(Player player) 
        {
            _movingToPlayer = true;
            _player = player;
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!_movingToPlayer)
                return;

            Position = Position.Lerp(_player.Position, 0.1f);

            if (Position.DistanceTo(_player.Position) < 20) 
            {
                _player.Gold++;
                QueueFree();
            }
        }
    }
}
