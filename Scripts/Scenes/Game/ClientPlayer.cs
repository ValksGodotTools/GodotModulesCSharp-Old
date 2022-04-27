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
        public static Timer EmitPosition { get; set; }

        public override void _Ready()
        {
            base._Ready();
            SetHealth(100);
            LabelPosition = GetNode<Label>(NodePathLabelPosition);

            if (GameClient.Running)
            {
                EmitPosition = new Timer(1000);
                EmitPosition.Elapsed += EmitPositionCallback;
                EmitPosition.AutoReset = true;
                EmitPosition.Enabled = true;
            }
        }

        public async void EmitPositionCallback(System.Object source, ElapsedEventArgs args)
        {
            await GameClient.Send(ClientPacketOpcode.PlayerPosition, new CPacketPlayerPosition {
                Position = Position
            });
        }

        public override void _PhysicsProcess(float delta)
        {
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
        }

        public void SetHealth(int v)
        {
            SceneGame.Instance.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }
}