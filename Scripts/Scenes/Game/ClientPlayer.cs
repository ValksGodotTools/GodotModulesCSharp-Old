using Timer = System.Timers.Timer;

using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System.Timers;
using System.Threading.Tasks;

namespace Game
{
    public class ClientPlayer : OtherPlayer
    {
        [Export] public float Speed = 250f;

        private Timer TimerNotifyServerClientPosition { get; set; }

        public override void _Ready()
        {
            base._Ready();
            SetHealth(100);

            if (GameClient.Running)
            {
                TimerNotifyServerClientPosition = new Timer(2000);
                TimerNotifyServerClientPosition.Elapsed += TimerNotifyServerClientPositionCallback;
                TimerNotifyServerClientPosition.AutoReset = true;
                TimerNotifyServerClientPosition.Enabled = true;
            }
        }

        public async void TimerNotifyServerClientPositionCallback(System.Object source, ElapsedEventArgs args)
        {
            await GameClient.Send(ClientPacketOpcode.PlayerPosition, new CPacketPlayerPosition
            {
                Position = Position
            });
        }

        public override void _Process(float delta)
        {
            HandleMovement(delta);
        }

        private async void HandleMovement(float delta)
        {
            var dir = new Vector2();

            if (Input.IsActionPressed("ui_left"))
                dir.x -= 1;
            if (Input.IsActionPressed("ui_right"))
                dir.x += 1;
            if (Input.IsActionPressed("ui_up"))
                dir.y -= 1;
            if (Input.IsActionPressed("ui_down"))
                dir.y += 1;

            Position += dir * Speed * delta;

            await SendDirection();
        }

        private async Task SendDirection()
        {
            if (!GameClient.Running)
                return;

            // PRESSED
            if (Input.IsActionJustPressed("ui_left"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionPressed, new CPacketPlayerDirectionPressed
                {
                    Direction = Direction.West
                });
            if (Input.IsActionJustPressed("ui_right"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionPressed, new CPacketPlayerDirectionPressed
                {
                    Direction = Direction.East
                });
            if (Input.IsActionJustPressed("ui_up"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionPressed, new CPacketPlayerDirectionPressed
                {
                    Direction = Direction.North
                });
            if (Input.IsActionJustPressed("ui_down"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionPressed, new CPacketPlayerDirectionPressed
                {
                    Direction = Direction.South
                });

            // RELEASED
            if (Input.IsActionJustReleased("ui_left"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionReleased, new CPacketPlayerDirectionReleased
                {
                    Direction = Direction.West
                });
            if (Input.IsActionJustReleased("ui_right"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionReleased, new CPacketPlayerDirectionReleased
                {
                    Direction = Direction.East
                });
            if (Input.IsActionJustReleased("ui_up"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionReleased, new CPacketPlayerDirectionReleased
                {
                    Direction = Direction.North
                });
            if (Input.IsActionJustReleased("ui_down"))
                await GameClient.Send(ClientPacketOpcode.PlayerDirectionReleased, new CPacketPlayerDirectionReleased
                {
                    Direction = Direction.South
                });
        }

        public void SetHealth(int v)
        {
            SceneGame.Instance.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }
}