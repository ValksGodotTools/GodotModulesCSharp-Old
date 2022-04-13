using Godot;
using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Valk.Modules.Settings;

namespace Valk.Modules.Netcode
{
    public class UICreateLobby : WindowDialog
    {
        [Export] public readonly NodePath NodePathInputTitle;
        [Export] public readonly NodePath NodePathInputPort;
        [Export] public readonly NodePath NodePathInputDescription;
        [Export] public readonly NodePath NodePathMaxPlayerCount;
        [Export] public readonly NodePath NodePathVBoxFeedback;

        private LineEdit InputTitle { get; set; }
        private LineEdit InputPort { get; set; }
        private TextEdit InputDescription { get; set; }
        private LineEdit InputMaxPlayerCount { get; set; }
        private VBoxContainer VBoxFeedback { get; set; }

        private static Dictionary<string, string> Feedback = new Dictionary<string, string>();

        public override void _Ready()
        {
            InputTitle = GetNode<LineEdit>(NodePathInputTitle);
            InputPort = GetNode<LineEdit>(NodePathInputPort);
            InputDescription = GetNode<TextEdit>(NodePathInputDescription);
            InputMaxPlayerCount = GetNode<LineEdit>(NodePathMaxPlayerCount);
            VBoxFeedback = GetNode<VBoxContainer>(NodePathVBoxFeedback);
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
            InputPort.Text = "25565";
            InputDescription.Text = "";
            InputMaxPlayerCount.Text = "10";
        }

        public void ClearFeedback() 
        {
            foreach (Control child in VBoxFeedback.GetChildren())
                child.QueueFree();
        }

        private void _on_Title_text_changed(string text) => Validate("title", text, "^[A-Za-z ]{3,15}$", "Name must contain only letters and have length of 3 to 15 characters");
        private void _on_Port_text_changed(string text) => Validate("port", text, "^[0-9]{1,65535}$", "Invalid Port: Please enter a valid number");
        private void _on_Description_text_changed() 
        {
            if (InputDescription.Text.Trim().Length > 250)
                Feedback["description"] = "Description exceeded max character limit of 250 characters";
            else
                Feedback["description"] = "";

            UpdateFeedback();
        }
        private void _on_Max_Player_text_changed(string text) 
        {
            if (!int.TryParse(text.Trim(), out int maxCount))
                Feedback["maxPlayers"] = "Invalid Max Players: Please enter a valid number";
            else if (maxCount <= 0 || maxCount >= 1000)
                Feedback["maxPlayers"] = "Invalid Max Players: Please enter a number between 1 and 999";
            else
                Feedback["maxPlayers"] = "";

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

        private string GetExternalIp() 
        {
            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return IPAddress.Parse(externalIpString).ToString();
        }

        private void _on_Create_pressed()
        {
            if (!IsValid())
                return;

            var lobbyPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyListing.tscn");

            var port = ushort.Parse(InputPort.Text.Trim());
            var externalIp = GetExternalIp();

            var lobby = lobbyPrefab.Instance<UILobbyListing>();
            lobby.Init();
            lobby.SetInfo(new LobbyListing {
                Name = InputTitle.Text.Trim(),
                Ip = externalIp,
                Port = port,
                Description = InputDescription.Text.Trim(),
                MaxPlayerCount = int.Parse(InputMaxPlayerCount.Text.Trim()),
                LobbyHost = UIOptions.Options.OnlineUsername
            });

            UIGameServers.ServerList.AddChild(lobby);

            GameManager.GameServer.Start();
            GameManager.GameClient.Connect(externalIp, port);

            UIGameServers.CurrentLobby = lobby.Info;
            GetTree().ChangeScene("res://Scenes/Lobby.tscn");

            Hide();
        }
    }
}
