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

        private static VBoxContainer ServerList { get; set; }
        public UICreateLobby ServerCreationPopup { get; set; }

        public static LobbyListing CurrentLobby { get; set; }
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
                WebClient.Client.CancelPendingRequests();
                GetTree().ChangeScene("res://Scenes/Menu.tscn");
            }
        }

        public static void AddServer(LobbyListing info)
        {
            var lobbyPrefab = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/LobbyListing.tscn");
            var lobby = lobbyPrefab.Instance<UILobbyListing>();
            lobby.Init();
            lobby.SetInfo(info);
            ServerList.AddChild(lobby);
        }

        public static void ClearServers()
        {
            foreach (Control child in ServerList.GetChildren())
                child.QueueFree();
        }

        public static async void ListServers()
        {
            WebClient.TaskGetServers = WebClient.Get<LobbyListing[]>("servers/get");
            var res = await WebClient.TaskGetServers;

            if (res.Status == WebServerStatus.ERROR)
            {
                return;
            }

            foreach (var server in res.Content)
                AddServer(server);
        }

        public static async void PostServer(LobbyListing info)
        {
            await WebClient.Post("servers/post", new Dictionary<string, string>
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