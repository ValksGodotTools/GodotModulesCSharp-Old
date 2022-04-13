using Godot;
using System;

namespace Valk.Modules.Netcode
{
    public class UIGameServers : Node
    {
        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        public static VBoxContainer ServerList { get; set; }
        public static UICreateLobby ServerCreationPopup { get; set; }

        public override void _Ready()
        {
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UICreateLobby>(NodePathServerCreationPopup);
        }
    }
}
