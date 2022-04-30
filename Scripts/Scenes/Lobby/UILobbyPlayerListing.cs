using Godot;

namespace GodotModules
{
    public class UILobbyPlayerListing : Control
    {
        [Export] public readonly NodePath NodePathName;
        [Export] public readonly NodePath NodePathStatus;

        private Label PlayerName { get; set; }
        private Label Status { get; set; }

        public string Username { get; private set; }
        public bool Ready { get; private set; }
        public bool Host { get; private set; }

        public override void _Ready()
        {
            PlayerName = GetNode<Label>(NodePathName);
            Status = GetNode<Label>(NodePathStatus);
        }

        public void SetUsername(string value)
        {
            PlayerName.Text = value;
            Username = value;
        }

        public void SetReady(bool value)
        {
            Status.Text = value ? "Ready" : "Not Ready";
            Ready = value;
        }

        public void SetHost(bool value)
        {
            Host = value;
        }
    }
}