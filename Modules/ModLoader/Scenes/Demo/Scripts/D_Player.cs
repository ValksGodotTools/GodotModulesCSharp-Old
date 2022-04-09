using Godot;

namespace D_Game
{
    public class D_Player : Sprite
    {
        [Export] public float Speed = 250f;
        public int Health = 100;

        public D_Player() 
        {
            SetHealth(100);
        }

        public override void _Process(float delta)
        {
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
            D_Master.LabelPlayerHealth.Text = $"Health: {v}";
        }
    }

}
