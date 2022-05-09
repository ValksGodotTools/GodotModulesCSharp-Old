using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System.Threading.Tasks;

namespace GodotModules
{
    public class UIPopupCreateLobby : WindowDialog
    {
        [Export] public readonly NodePath NodePathInputTitle;
        [Export] public readonly NodePath NodePathInputPort;
        [Export] public readonly NodePath NodePathInputDescription;
        [Export] public readonly NodePath NodePathMaxPlayerCount;
        [Export] public readonly NodePath NodePathVBoxFeedback;
        [Export] public readonly NodePath NodePathPublic;
        [Export] public readonly NodePath NodePathNumPingChecks;
        [Export] public readonly NodePath NodePathNumPingChecksEnabled;

        private LineEdit InputTitle { get; set; }
        private LineEdit InputPort { get; set; }
        private TextEdit InputDescription { get; set; }
        private LineEdit InputMaxPlayerCount { get; set; }
        private VBoxContainer VBoxFeedback { get; set; }
        private CheckBox Public { get; set; }
        private LineEdit NumPingChecks { get; set; }
        private CheckBox NumPingChecksEnabled { get; set; }
        private SceneGameServers SceneGameServersScript { get; set; }

        public override void _Ready()
        {
            SceneGameServersScript = SceneManager.GetActiveSceneScript<SceneGameServers>();
            InputTitle = GetNode<LineEdit>(NodePathInputTitle);
            InputPort = GetNode<LineEdit>(NodePathInputPort);
            InputDescription = GetNode<TextEdit>(NodePathInputDescription);
            InputMaxPlayerCount = GetNode<LineEdit>(NodePathMaxPlayerCount);
            VBoxFeedback = GetNode<VBoxContainer>(NodePathVBoxFeedback);
            Public = GetNode<CheckBox>(NodePathPublic);
            NumPingChecks = GetNode<LineEdit>(NodePathNumPingChecks);
            NumPingChecksEnabled = GetNode<CheckBox>(NodePathNumPingChecksEnabled);

            ValidatedName = InputTitle.Text;
            ValidatedDescription = InputDescription.Text;
            ValidatedPort = int.Parse(InputPort.Text);
            ValidatedMaxPlayerCount = int.Parse(InputMaxPlayerCount.Text);
        }

        private const int MAX_PLAYER_COUNT = 256;

        public void ClearFields()
        {
            InputTitle.Text = "Another lobby";
            InputPort.Text = "25565";
            InputDescription.Text = "";
            InputMaxPlayerCount.Text = $"{MAX_PLAYER_COUNT}"; // byte values range from 0-255 (gives 256 ids) (should be noted dummy clients connect to ping servers and take up an id for a short duration)
        }

        private int ValidatedMaxPlayerCount;
        private int ValidatedPort;
        private string ValidatedName { get; set; }
        private string ValidatedDescription { get; set; }

        private string previousTextTitle = "";
        private string previousTextDescription = "";

        private void _on_Title_text_changed(string text) =>
            ValidatedName = text.Validate(ref previousTextTitle, InputTitle, () => text.IsMatch("^[A-Za-z ]+$"));

        private void _on_Port_text_changed(string text) => text.ValidateNumber(InputPort, 1, 65535, ref ValidatedPort);

        private void _on_Max_Player_text_changed(string text) => text.ValidateNumber(InputMaxPlayerCount, 1, MAX_PLAYER_COUNT, ref ValidatedMaxPlayerCount);

        private void _on_Description_text_changed() =>
            ValidatedDescription = InputDescription.Text.Validate(ref previousTextDescription, InputDescription, () => InputDescription.Text.Length <= 250);

        private async void _on_Create_pressed()
        {
            if (GameClient.ConnectingToLobby)
                return;

            if (ValidatedName == null || ValidatedDescription == null)
                return;

            var port = (ushort)ValidatedPort;
            var localIp = "127.0.0.1";
            var name = InputTitle.Text.Trim();
            var desc = InputDescription.Text.Trim();

            Hide();

            NetworkManager.StartServer(port, ValidatedMaxPlayerCount);

            var dummyClient = new ENetClient();
            dummyClient.Start(WebClient.ExternalIp, port);
            int attempts = 100;
            while (!dummyClient.IsConnected) 
            {
                await Task.Delay(1);
                attempts--;

                if (attempts == 0)
                {
                    GameManager.SpawnPopupMessage($"The port '{port}' must be port forwarded first");
                    NetworkManager.GameServer.Stop();
                    dummyClient.Stop();
                    return;
                }
            }
            dummyClient.Stop();

            NetworkManager.StartClient(localIp, port);

            var info = new LobbyListing
            {
                Name = name,
                Ip = localIp,
                Port = port,
                Description = desc,
                MaxPlayerCount = ValidatedMaxPlayerCount,
                LobbyHost = GameManager.Options.OnlineUsername,
                Public = Public.Pressed
            };

            SceneGameServersScript.AddServer(info);
            if (WebClient.ConnectionAlive)
                SceneGameServersScript.PostServer(info);
            SceneLobby.CurrentLobby = info;

            await SceneGameServersScript.ClientConnect(async () => {
                await NetworkManager.WaitForHostToConnectToServer();
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby(LobbyOpcode.LobbyCreate)
                {
                    Username = GameManager.Options.OnlineUsername,
                    LobbyName = name,
                    LobbyDescription = desc
                });
            });
        }
    }
}