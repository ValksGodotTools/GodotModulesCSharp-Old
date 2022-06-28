namespace GodotModules
{
    public class UIPlayerListing : Node
    {
        [Export] protected readonly NodePath NodePathName;
        [Export] protected readonly NodePath NodePathKick;

        private Label _name;
        private Button _kick;

        public string PlayerName { get; private set; }

        public override void _Ready()
        {
            _name = GetNode<Label>(NodePathName);
            _kick = GetNode<Button>(NodePathKick);
        }

        public void SetPlayerName(string name) 
        {
            PlayerName = name;
            _name.Text = name;
        }

        public void HideKickBtn() => _kick.Visible = false;
        public void ShowKickBtn() => _kick.Visible = true;

        private void _on_Kick_pressed()
        {
            Logger.Log("Kick");
        }
    }
}
