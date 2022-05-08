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

        private bool PressedUp { get; set; }
        private bool PressedDown { get; set; }
        private bool PressedLeft { get; set; }
        private bool PressedRight { get; set; }

        private SceneGame SceneGameScript { get; set; }

        public override void _Ready()
        {
            base._Ready();
            SceneGameScript = SceneManager.GetActiveSceneScript<SceneGame>();
            SetHealth(100);
            LabelPosition = GetNode<Label>(NodePathLabelPosition);

            if (NetworkManager.IsMultiplayer())
            {
                if (NetworkManager.ServerAuthoritativeMovement)
                {
                    var timer1 = new GTimer(ClientIntervals.PlayerDirection);
                    timer1.Connect(this, nameof(EmitMovementDirection));
                }
                else 
                {
                    var timer1 = new GTimer(ClientIntervals.PlayerPosition);
                    timer1.Connect(this, nameof(EmitPosition));
                }

                var timer2 = new GTimer(ClientIntervals.PlayerRotation);
                timer2.Connect(this, nameof(EmitRotation));
            }
        }

        public async void EmitPosition() 
        {
            await NetworkManager.GameClient.Send(ClientPacketOpcode.PlayerPosition, new CPacketPlayerPosition {
                Position = Position
            }, ENet.PacketFlags.None);
        }

        public async void EmitMovementDirection()
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

        public async void EmitRotation()
        {
            await NetworkManager.GameClient.Send(ClientPacketOpcode.PlayerRotation, new CPacketPlayerRotation {
                Rotation = PlayerSprite.Rotation
            }, ENet.PacketFlags.None);
        }

        public override void _PhysicsProcess(float delta)
        {
            LabelPosition.Text = $"X: {Mathf.RoundToInt(Position.x)} Y: {Mathf.RoundToInt(Position.y)}";

            PlayerSprite.LookAt(GetGlobalMousePosition());

            HandleMovement(delta);

            if (NetworkManager.IsMultiplayer() && NetworkManager.ServerAuthoritativeMovement)
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

            //Position += dir * Speed * delta;
            MoveAndSlide(dir * Speed * delta * 50);
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
            SceneGameScript.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }
}