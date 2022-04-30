using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;

namespace Game
{
    public class ClientPlayer : OtherPlayer
    {
        [Export] public readonly NodePath NodePathLabelPosition;
        private Label LabelPosition { get; set; }

        private float Speed = 250f;
        public static GTimer Timer { get; set; }

        private bool PressedUp { get; set; }
        private bool PressedDown { get; set; }
        private bool PressedLeft { get; set; }
        private bool PressedRight { get; set; }

        public override void _Ready()
        {
            base._Ready();
            SetHealth(100);
            LabelPosition = GetNode<Label>(NodePathLabelPosition);

            if (NetworkManager.GameClient.Running)
            {
                Timer = new GTimer(20);
                Timer.Connect(this, nameof(EmitInput));
            }
        }

        public async void EmitInput()
        {
            var directionVert = Direction.None;
            var directionHorz = Direction.None;

            if (PressedUp) directionVert = Direction.Up;
            if (PressedDown) directionVert = Direction.Down;
            if (PressedLeft) directionHorz = Direction.Left;
            if (PressedRight) directionHorz = Direction.Right;

            await NetworkManager.GameClient.Send(ClientPacketOpcode.PlayerMovementDirections, new CPacketPlayerMovementDirections
            {
                DirectionVert = directionVert,
                DirectionHorz = directionHorz
            }, ENet.PacketFlags.None);
        }

        public override void _PhysicsProcess(float delta)
        {
            LabelPosition.Text = $"X: {Mathf.RoundToInt(Position.x)} Y: {Mathf.RoundToInt(Position.y)}";
            HandleMovement(delta);

            if (NetworkManager.GameClient.Running)
                KeepTrackOfInputs();
        }

        private void HandleMovement(float delta)
        {
            var dir = new Vector2();

            if (Input.IsActionPressed("ui_up"))
                dir.y -= 1;
            if (Input.IsActionPressed("ui_down"))
                dir.y += 1;
            if (Input.IsActionPressed("ui_left"))
                dir.x -= 1;
            if (Input.IsActionPressed("ui_right"))
                dir.x += 1;

            Position += dir * Speed * delta;
        }

        private void KeepTrackOfInputs()
        {
            PressedUp = Input.IsActionPressed("ui_up");
            PressedDown = Input.IsActionPressed("ui_down");
            PressedLeft = Input.IsActionPressed("ui_left");
            PressedRight = Input.IsActionPressed("ui_right");
        }

        public void SetHealth(int v)
        {
            SceneGame.Instance.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }
}