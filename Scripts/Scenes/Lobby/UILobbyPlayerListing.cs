using Godot;

namespace GodotModules.Netcode
{
    public class UILobbyPlayerListing : Control
    {
        [Export] public readonly NodePath NodePathName;
        [Export] public readonly NodePath NodePathStatus;

        private Label PlayerName { get; set; }
        public Label Status { get; set; }

        public LobbyPlayerListing Info { get; set; }

        public void Init()
        {
            PlayerName = GetNode<Label>(NodePathName);
            Status = GetNode<Label>(NodePathStatus);
        }

        public void SetInfo(LobbyPlayerListing info)
        {
            Info = info;
            PlayerName.Text = info.Name;
            Status.Text = info.Ready ? "Ready" : "Not Ready";
        }
    }

    public struct LobbyPlayerListing
    {
        public string Name { get; set; }
        public bool Ready { get; set; }
    }
}