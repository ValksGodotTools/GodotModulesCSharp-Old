using Godot;

namespace Game
{
    public class OtherPlayer : KinematicBody2D
    {
        [Export] public readonly NodePath NodePathPlayerSprite;
        [Export] public readonly NodePath NodePathLabelUsername;

        public Sprite PlayerSprite { get; set; }
        private Label LabelUsername { get; set; }

        public int Health = 100;

        public override void _Ready()
        {
            PlayerSprite = GetNode<Sprite>(NodePathPlayerSprite);
            LabelUsername = GetNode<Label>(NodePathLabelUsername);
        }

        public void SetUsername(string username)
        {
            LabelUsername.Text = username;
        }
    }
}