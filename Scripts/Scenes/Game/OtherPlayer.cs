using Godot;

namespace Game
{
    public class OtherPlayer : Sprite
    {
        [Export] public readonly NodePath NodePathLabelUsername;

        private Label LabelUsername { get; set; }

        public int Health = 100;

        public override void _Ready()
        {
            LabelUsername = GetNode<Label>(NodePathLabelUsername);
        }

        public void SetUsername(string username)
        {
            LabelUsername.Text = username;
        }
    }
}