using Godot;
using System;

namespace Valk.Modules.Netcode
{
    public class UIGameServers : Control
    {
        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        public static VBoxContainer ServerList { get; set; }
        public static UICreateLobby ServerCreationPopup { get; set; }

        public static LobbyListing CurrentLobby { get; set; }

        public override void _Ready()
        {
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UICreateLobby>(NodePathServerCreationPopup);
        }

        private void _on_Control_resized()
        {
            if (ServerCreationPopup != null)
                ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }
    }
}
