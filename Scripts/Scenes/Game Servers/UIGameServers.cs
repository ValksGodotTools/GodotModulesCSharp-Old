using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            ListServers();
        }

        public static void AddServer(LobbyListing info)
        {
            var lobbyPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyListing.tscn");
            var lobby = lobbyPrefab.Instance<UILobbyListing>();
            lobby.Init();
            lobby.SetInfo(info);
            ServerList.AddChild(lobby);
            PostServer(info);
        }

        private static async void ListServers() 
        {
            var servers = await WebClient.Get<LobbyListing[]>("localhost:4000/api/servers/get");

            if (servers.Status == WebServerStatus.ERROR)
                return;

            foreach (var server in servers.Content)
                AddServer(server);
        }

        private static async void PostServer(LobbyListing info)
        {
            await WebClient.Post("localhost:4000/api/servers/post", new Dictionary<string, string>
            {
                { "Name", info.Name },
                { "Ip", info.Ip },
                { "Port", "" + info.Port },
                { "Description", info.Description },
                { "MaxPlayerCount", "" + info.MaxPlayerCount }
            });
        }

        private void _on_Control_resized()
        {
            if (ServerCreationPopup != null)
                ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }
    }
}
