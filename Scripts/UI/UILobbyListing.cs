using Godot;
using System;
using System.Collections.Generic;

namespace Valk.Modules.Netcode
{
    public class UILobbyListing : Control
    {
        [Export] public readonly NodePath NodePathLabelTitle;
        [Export] public readonly NodePath NodePathLabelDescription;
        [Export] public readonly NodePath NodePathLabelAddress;
        [Export] public readonly NodePath NodePathLabelPing;
        [Export] public readonly NodePath NodePathLabelPlayerCount;
        [Export] public readonly NodePath NodePathButtonLobby;

        private Label LabelTitle { get; set; }
        private Label LabelDescription { get; set; }
        private Label LabelAddress { get; set; }
        private Label LabelPing { get; set; }
        private Label LabelPlayerCount { get; set; }
        private Button ButtonLobby { get; set; }

        public string Title => LabelTitle.Text;
        public string Description => LabelDescription.Text;
        public string Address => $"{Ip}:{Port}";
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayerCount { get; set; }

        public static UILobbyListing CurrentListingFocused { get; set; }

        public void Init()
        {
            LabelTitle = GetNode<Label>(NodePathLabelTitle);
            LabelDescription = GetNode<Label>(NodePathLabelDescription);
            LabelAddress = GetNode<Label>(NodePathLabelAddress);
            LabelPing = GetNode<Label>(NodePathLabelPing);
            LabelPlayerCount = GetNode<Label>(NodePathLabelPlayerCount);
            ButtonLobby = GetNode<Button>(NodePathButtonLobby);
        }

        public void SetTitle(string text) => LabelTitle.Text = text;
        public void SetDescription(string text) => LabelDescription.Text = text;
        public void SetAddress(string ip, ushort port) 
        {
            Ip = ip;
            Port = port;
            LabelAddress.Text = $"{ip}:{port}";
        }
        public void SetPing(int ping) => LabelPing.Text = $"{ping} ms";
        public void SetMaxPlayerCount(int count) => MaxPlayerCount = count;
        public void SetPlayerCount(int count) 
        {
            PlayerCount = count;
            LabelPlayerCount.Text = $"{PlayerCount} / {MaxPlayerCount}";
        }

        public void Join()
        {
            GameManager.GameClient.Connect(Ip, Port);
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
}
