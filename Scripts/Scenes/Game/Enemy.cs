using Godot;

namespace GodotModules.Netcode.Server
{
    public class Enemy : RigidBody2D
    {
        [Export] public readonly NodePath NodePathSprite;
        private Sprite Sprite { get; set; }

        private Vector2 Velocity { get; set; }
        private float Delta { get; set; }

        public override void _Ready()
        {
            Sprite = GetNode<Sprite>(NodePathSprite);

            var calcTarget = new GTimer(200);
            calcTarget.Connect(this, nameof(CalcVelocity));
        }

        public override void _PhysicsProcess(float delta)
        {
            Delta = delta;

            if (Players.Count == 0) 
                return;

            Sprite.LookAt(Players.First().Value.Position);
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            state.LinearVelocity = Velocity;
        }

        public Dictionary<byte, Game.OtherPlayer> Players { get; set; }

        public void SetPlayers(Dictionary<byte, Game.OtherPlayer> players) 
        {
            Players = players;
        }

        private void CalcVelocity()
        {
            if (Players.Count == 0) 
            {
                Velocity = Vector2.Zero;
                return;
            }

            // TODO: Find nearest player instead of first player in array
            var dir = (Players.First().Value.Position - Position).Normalized();
            Velocity = (dir * Delta * 10000f);
        }

        private void _on_Hit_Zone_area_entered(Area2D area)
        {

        }

        private void _on_Hit_Zone_area_exited(Area2D area)
        {

        }
    }
}
