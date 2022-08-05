namespace GodotModules;

public class Coin : AnimatedSprite
{
    private bool _movingToPlayer;
    private bool _jumpingOutOfChest;
    private Vector2 _chestPos;
    private Vector2 _targetJumpPos;
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

    public void JumpOutOfChest(Vector2 chestPos, Vector2 pos)
    {
        _jumpingOutOfChest = true;
        _chestPos = chestPos;
        Position = chestPos;
        _targetJumpPos = pos;
    }

    public override void _PhysicsProcess(float delta)
    {
        MovingToPlayer();
        JumpingOutOfChest();
    }

    private void MovingToPlayer()
    {
        if (!_movingToPlayer || _jumpingOutOfChest)
            return;

        Position = Position.Lerp(_player.Position, 0.05f);

        if (Position.DistanceTo(_player.Position) < 20)
        {
            _player.Gold++;
            QueueFree();
        }
    }

    private void JumpingOutOfChest()
    {
        if (!_jumpingOutOfChest)
            return;

        Position = Position.Lerp(_targetJumpPos, 0.1f);

        if (Position.DistanceTo(_targetJumpPos) < 5)
        {
            _jumpingOutOfChest = false;
        }
    }
}
