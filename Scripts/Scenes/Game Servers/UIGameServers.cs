using Godot;
using GodotModules.Netcode;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodotModules.Netcode
{
    public class UIGameServers : Control
    {
        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        private VBoxContainer ServerList { get; set; }
        public UICreateLobby ServerCreationPopup { get; set; }

        public LobbyListing CurrentLobby { get; set; }
        public static UIGameServers Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UICreateLobby>(NodePathServerCreationPopup);
            ListServers();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GameManager.WebClient.Client.CancelPendingRequests();
                GameManager.ChangeScene("Menu");
            }
        }

        public void AddServer(LobbyListing info)
        {
            var lobbyPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyListing.tscn");
            var lobby = lobbyPrefab.Instance<UILobbyListing>();
            lobby.Init();
            lobby.SetInfo(info);
            ServerList.AddChild(lobby);
        }

        public void ClearServers()
        {
            foreach (Control child in ServerList.GetChildren())
                child.QueueFree();
        }

        public async void ListServers()
        {
            GameManager.WebClient.TaskGetServers = GameManager.WebClient.Get<LobbyListing[]>("servers/get");
            var res = await GameManager.WebClient.TaskGetServers;

            if (res.Status == WebServerStatus.ERROR)
            {
                return;
            }

            foreach (var server in res.Content)
                AddServer(server);
        }

        public async void PostServer(LobbyListing info)
        {
            var res = await GameManager.WebClient.Post("servers/post", new Dictionary<string, string>
            {
                { "Name", info.Name },
                { "Ip", info.Ip },
                { "Port", "" + info.Port },
                { "Description", info.Description },
                { "MaxPlayerCount", "" + info.MaxPlayerCount }
            });

            if (res.Status == WebServerStatus.ERROR)
            {
                // TODO: Try to post server on master server 3 more times
            }
        }

        private void _on_Control_resized()
        {
            if (ServerCreationPopup != null)
                ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }
    }
}