using Godot;

namespace GodotModules.Netcode
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

        public UILobbyListing CurrentListingFocused { get; set; }
        public static UILobbyListing Instance { get; set; }

        public void Init()
        {
            Instance = this;
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

        public void Join()
        {
            GD.Print($"Connecting to {Info.Ip}:{Info.Port}");
            //GameManager.GameClient.Connect(Info.Ip, Info.Port);
            GameManager.StartClient(Info.Ip, Info.Port);
        }

        private void _on_Btn_pressed()
        {
            CurrentListingFocused = this;
            Join();
        }

        private void _on_Btn_focus_entered()
        {
            CurrentListingFocused = this;
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
        public int NumPingChecks { get; set; }
        public bool NumPingChecksEnabled { get; set; }
    }
}