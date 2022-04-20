using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Settings;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GodotModules
{
    public class UICreateLobby : WindowDialog
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
        }

        private Label AddFeedback(string text)
        {
            var label = new Label { Text = text };
            VBoxFeedback.AddChild(label);
            return label;
        }

        public void ClearFields()
        {
            InputTitle.Text = "Test";
            InputPort.Text = "7777";
            InputDescription.Text = "";
            InputMaxPlayerCount.Text = "10";
        }

        public void ClearFeedback()
        {
            foreach (Control child in VBoxFeedback.GetChildren())
                child.QueueFree();
        }

        private int ValidatedMaxPlayerCount = 10;
        private int ValidatedNumPingAttempts = 3;
        private int ValidatedPort = 7777;

        private void _on_Title_text_changed(string text) => Validate("title", text, "^[A-Za-z ]{3,15}$", "Name must contain only letters and have length of 3 to 15 characters");

        private void _on_Port_text_changed(string text) => Validator.ValidateNumber(InputPort, text, 65535, ref ValidatedPort);

        private void _on_Max_Player_text_changed(string text) => Validator.ValidateNumber(InputMaxPlayerCount, text, 999, ref ValidatedMaxPlayerCount);

        private void _on_NumAttempts_text_changed(string text) => Validator.ValidateNumber(NumPingChecks, text, 99, ref ValidatedNumPingAttempts);

        private void _on_Description_text_changed()
        {
            if (InputDescription.Text.Trim().Length > 250)
                Feedback["description"] = "Description exceeded max character limit of 250 characters";
            else
                Feedback["description"] = "";

            UpdateFeedback();
        }

        private void UpdateFeedback()
        {
            ClearFeedback();

            foreach (var text in Feedback.Values)
                if (text != "")
                    AddFeedback(text);
        }

        private bool IsValid() => VBoxFeedback.GetChildren().Count == 0;

        private void Validate(string key, string text, string pattern, string feedback)
        {
            if (!Regex.IsMatch(text.Trim(), pattern))
                Feedback[key] = feedback;
            else
                Feedback[key] = "";

            UpdateFeedback();
        }

        private async void _on_Create_pressed()
        {
            if (SceneGameServers.ConnectingToLobby)
                return;

            if (!IsValid())
                return;

            var port = ushort.Parse(InputPort.Text.Trim());
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
                Public = Public.Pressed,
                NumPingChecks = ValidatedNumPingAttempts,
                NumPingChecksEnabled = NumPingChecksEnabled.Pressed
            };

            SceneGameServers.Instance.AddServer(info);
            SceneGameServers.Instance.PostServer(info);
            SceneLobby.CurrentLobby = info;
            
            Hide();

            NetworkManager.StartServer(port, ValidatedMaxPlayerCount);
            NetworkManager.StartClient(localIp, port);
            await NetworkManager.ClientConnecting();
            await NetworkManager.WaitForHostToConnectToServer();
            await ENetClient.Send(ClientPacketOpcode.LobbyJoin, new WPacketLobbyJoin {
                Username = GameManager.Options.OnlineUsername
            });
        }
    }
}