using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace GodotModules
{
    public class SceneGameServers : Control
    {
        public static Dictionary<string, LobbyListing> LobbyListings { get; set; }
        public static SceneGameServers Instance { get; set; }
        public static UILobbyListing SelectedLobbyInstance { get; set; }
        public static bool ConnectingToLobby { get; set; }
        public static bool Disconnected { get; set; }

        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        private VBoxContainer ServerList { get; set; }
        public UIPopupCreateLobby ServerCreationPopup { get; set; }

        public override async void _Ready()
        {
            Instance = this;
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UIPopupCreateLobby>(NodePathServerCreationPopup);
            LobbyListings = new();

            if (Disconnected)
            {
                Disconnected = false;
                var message = "Disconnected";

                switch (ENetClient.DisconnectOpcode)
                {
                    case DisconnectOpcode.Timeout:
                        message = "Timed out from server";
                        break;
                    case DisconnectOpcode.Restarting:
                        message = "Server is restarting..";
                        break;
                    case DisconnectOpcode.Stopping:
                        message = "Server was stopped";
                        break;
                    case DisconnectOpcode.Kicked:
                        message = "You were kicked";
                        break;
                    case DisconnectOpcode.Banned:
                        message = "You were banned";
                        break;
                }

                GameManager.SpawnPopupMessage(message);
            }

            await ListServers();
        }

        public override void _Input(InputEvent @event)
        {
            Utils.EscapeToScene("Menu", () =>
            {
                NetworkManager.WebClient.Client.CancelPendingRequests();
                NetworkManager.CancelClientConnectingTokenSource();
            });
        }

        public static async Task JoinServer(string ip, ushort port)
        {
            if (SceneGameServers.ConnectingToLobby)
                return;

            SceneGameServers.ConnectingToLobby = true;

            GD.Print("Connecting to lobby...");
            NetworkManager.StartClient(ip, port);

            await NetworkManager.WaitForClientToConnect(3000, async () =>
            {
                await ENetClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyJoin,
                    Username = GameManager.Options.OnlineUsername
                });
            });
        }

        public void AddServer(LobbyListing info)
        {
            var lobby = Prefabs.LobbyListing.Instance<UILobbyListing>();
            ServerList.AddChild(lobby);
            lobby.SetInfo(info);
        }

        public void ClearServers()
        {
            foreach (Control child in ServerList.GetChildren())
                child.QueueFree();
        }

        public async Task ListServers()
        {
            NetworkManager.WebClient.TaskGetServers = NetworkManager.WebClient.Get<LobbyListing[]>("servers/get");
            var res = await NetworkManager.WebClient.TaskGetServers;

            if (res.Status == WebServerStatus.ERROR)
                return;

            LobbyListings.Clear();

            res.Content.ForEach(server => {
                LobbyListings.Add(server.Ip, server);
                AddServer(server);
            });
        }

        public async void PostServer(LobbyListing info)
        {
            var res = await NetworkManager.WebClient.Post("servers/post", new Dictionary<string, string>
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