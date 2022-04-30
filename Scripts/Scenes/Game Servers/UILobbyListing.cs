using Godot;
using System.Threading.Tasks;

namespace GodotModules
{
    public class UILobbyListing : Control
    {
        [Export] public readonly NodePath NodePathLabelTitle;
        [Export] public readonly NodePath NodePathLabelDescription;
        [Export] public readonly NodePath NodePathLabelPing;
        [Export] public readonly NodePath NodePathLabelPlayerCount;
        [Export] public readonly NodePath NodePathButtonLobby;

        private Label LabelTitle { get; set; }
        private Label LabelDescription { get; set; }
        private Label LabelPing { get; set; }
        private Label LabelPlayerCount { get; set; }
        private Button ButtonLobby { get; set; }

        public LobbyListing Info { get; set; }

        public override void _Ready()
        {
            SceneGameServers.ConnectingToLobby = false;
            LabelTitle = GetNode<Label>(NodePathLabelTitle);
            LabelDescription = GetNode<Label>(NodePathLabelDescription);
            LabelPing = GetNode<Label>(NodePathLabelPing);
            LabelPlayerCount = GetNode<Label>(NodePathLabelPlayerCount);
            ButtonLobby = GetNode<Button>(NodePathButtonLobby);
        }

        public void SetInfo(LobbyListing info)
        {
            Info = info;
            LabelTitle.Text = info.Name;
            LabelDescription.Text = info.Description;
            LabelPlayerCount.Text = "" + info.MaxPlayerCount;
        }

        public async Task Join()
        {
            await SceneGameServers.JoinServer(Info.Ip, Info.Port);
        }

        private async void _on_Btn_pressed()
        {
            SceneGameServers.SelectedLobbyInstance = this;
            await Join();
        }

        private void _on_Btn_focus_entered()
        {
            SceneGameServers.SelectedLobbyInstance = this;
        }
    }

    public class LobbyListing
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public string LobbyHost { get; set; }
        public int MaxPlayerCount { get; set; }
        public bool Public { get; set; }
    }
}