using Godot;

namespace GodotModules
{
    public class UILobbyPlayerListing : Control
    {
        [Export] public readonly NodePath NodePathName;
        [Export] public readonly NodePath NodePathStatus;
        [Export] public readonly NodePath NodePathKick;

        private Label PlayerName { get; set; }
        private Label Status { get; set; }
        private Button Kick { get; set; }

        public string Username { get; private set; }
        public bool Ready { get; private set; }
        public bool Host { get; private set; }
        public uint Id { get; private set; }

        public override void _Ready()
        {
            PlayerName = GetNode<Label>(NodePathName);
            Status = GetNode<Label>(NodePathStatus);
            Kick = GetNode<Button>(NodePathKick);

            // Only host may kick others
            if (!NetworkManager.IsHost)
                Kick.Visible = false;
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

        public void SetId(uint value) 
        {
            Id = value;

            // host cannot kick self
            if (NetworkManager.PeerId == Id)
                Kick.Visible = false;
        }

        private async void _on_Kick_pressed()
        {
            SceneManager.GetActiveSceneScript<SceneLobby>().RemovePlayer(Id);
            NetworkManager.GameServer.Players.Remove((byte)Id);

            await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby(LobbyOpcode.LobbyKick) {
                Id = (byte)Id
            });
        }
    }
}