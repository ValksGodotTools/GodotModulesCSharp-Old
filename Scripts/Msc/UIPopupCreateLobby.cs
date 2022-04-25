using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        private Dictionary<string, string> Feedback = new Dictionary<string, string>();

        public override void _Ready()
        {
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

        public void ClearFields()
        {
            InputTitle.Text = "Test";
            InputPort.Text = "7777";
            InputDescription.Text = "";
            InputMaxPlayerCount.Text = "10";
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
        private void _on_Max_Player_text_changed(string text) => text.ValidateNumber(InputMaxPlayerCount, 1, 999, ref ValidatedMaxPlayerCount);
        private void _on_Description_text_changed() =>
            ValidatedDescription = InputDescription.Text.Validate(ref previousTextDescription, InputDescription, () => InputDescription.Text.Length <= 250);

        private async void _on_Create_pressed()
        {
            if (SceneGameServers.ConnectingToLobby)
                return;

            if (ValidatedName == null || ValidatedDescription == null)
                return;

            var port = (ushort)ValidatedPort;
            var localIp = "127.0.0.1";

            // does not update fast enough to be reliable
            /*if (UIGameServers.LobbyListings.ContainsKey(externalIp)) 
            {
                var listing = UIGameServers.LobbyListings[externalIp];

                if (listing.Port == port)
                {
                    GD.Print("A server is running on this ip and port already");
                    return;
                }
            }*/

            var info = new LobbyListing
            {
                Name = InputTitle.Text.Trim(),
                Ip = localIp,
                Port = port,
                Description = InputDescription.Text.Trim(),
                MaxPlayerCount = ValidatedMaxPlayerCount,
                LobbyHost = GameManager.Options.OnlineUsername,
                Public = Public.Pressed
            };

            SceneGameServers.Instance.AddServer(info);
            SceneGameServers.Instance.PostServer(info);
            SceneLobby.CurrentLobby = info;

            Hide();

            NetworkManager.StartServer(port, ValidatedMaxPlayerCount);
            NetworkManager.StartClient(localIp, port);

            await NetworkManager.WaitForClientToConnect(3000, async () =>
            {
                await NetworkManager.WaitForHostToConnectToServer();
                await ENetClient.Send(ClientPacketOpcode.LobbyJoin, new CPacketLobbyJoin
                {
                    Username = GameManager.Options.OnlineUsername
                });
            });
        }
    }
}