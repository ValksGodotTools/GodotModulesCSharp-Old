using Timer = System.Timers.Timer;

using ENet;
using Godot;
using GodotModules;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System.Timers;
using System.Threading.Tasks;

namespace Game
{
    public class ClientPlayer : OtherPlayer
    {
        [Export] public readonly NodePath NodePathLabelPosition;
        private Label LabelPosition { get; set; }

        private float Speed = 250f;
        private Timer NotifyServerPlayerDirection { get; set; }
        private float Delta { get; set; }
        private Direction DirectionHorizontal { get; set; }
        private Direction DirectionVertical { get; set; }

        public override void _Ready()
        {
            base._Ready();
            SetHealth(100);
            LabelPosition = GetNode<Label>(NodePathLabelPosition);

            if (GameClient.Running)
            {
                NotifyServerPlayerDirection = new Timer(50);
                NotifyServerPlayerDirection.Elapsed += NotifyServerPlayerDirectionCallback;
                NotifyServerPlayerDirection.AutoReset = true;
                NotifyServerPlayerDirection.Enabled = true;
            }
        }

        private Direction prevHorz;
        private Direction prevVert;

        public async void NotifyServerPlayerDirectionCallback(System.Object source, ElapsedEventArgs args)
        {
            //if (DirectionHorizontal == Direction.None && DirectionVertical == Direction.None) 
                //return;

            await GameClient.Send(ClientPacketOpcode.PlayerDirectionPressed, new CPacketPlayerDirectionPressed
            {
                DirectionHorizontal = DirectionHorizontal,
                DirectionVertical = DirectionVertical
            });
        }

        public override void _PhysicsProcess(float delta)
        {
            Delta = delta;
            LabelPosition.Text = $"X: {Mathf.RoundToInt(Position.x)} Y: {Mathf.RoundToInt(Position.y)}";
            HandleMovement(delta);
        }

        private void HandleMovement(float delta)
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

            if (DirectionHorizontal == Direction.None && DirectionVertical == Direction.None)
                Position = Utils.Lerp(Position, SceneGame.ServerPlayerPosition, 0.01f);
            //else
                //Position = Utils.Lerp(Position, SceneGame.ServerPlayerPosition, 0.05f);

            UpdateDirectionPressed();
        }

        private void UpdateDirectionPressed()
        {
            if (!GameClient.Running)
                return;

            // PRESSED
            if (Input.IsActionJustPressed("ui_left"))
                DirectionHorizontal = Direction.West;
            if (Input.IsActionJustPressed("ui_right"))
                DirectionHorizontal = Direction.East;
            if (Input.IsActionJustPressed("ui_up"))
                DirectionVertical = Direction.North;
            if (Input.IsActionJustPressed("ui_down"))
                DirectionVertical = Direction.South;

            // RELEASED
            if (Input.IsActionJustReleased("ui_left"))
                DirectionHorizontal = Direction.None;
            if (Input.IsActionJustReleased("ui_right"))
                DirectionHorizontal = Direction.None;
            if (Input.IsActionJustReleased("ui_up"))
                DirectionVertical = Direction.None;
            if (Input.IsActionJustReleased("ui_down"))
                DirectionVertical = Direction.None;
        }

        public void SetHealth(int v)
        {
            SceneGame.Instance.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }
}