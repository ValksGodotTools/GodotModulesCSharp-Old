using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    public class SceneGameServers : Control
    {
        public static Dictionary<string, LobbyListing> LobbyListings { get; set; }  // TODO: Check out stat ic here
        public static SceneGameServers Instance { get; set; }
        public static UILobbyListing SelectedLobbyInstance { get; set; }
        public static bool ConnectingToLobby { get; set; }
        public static bool Disconnected { get; set; }
        public static bool GettingServers { get; set; }

        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        private VBoxContainer ServerList { get; set; }
        public UIPopupCreateLobby ServerCreationPopup { get; set; }

        public override async void _Ready()
        {
            SceneGameServers.GettingServers = true; // because we await GetServers() at bottom
            UIGameServersNavBtns.BtnRefresh.Disabled = true;

            Instance = this;
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UIPopupCreateLobby>(NodePathServerCreationPopup);
            LobbyListings = new();

            if (Disconnected)
            {
                Disconnected = false;
                var message = "Disconnected";

                switch (NetworkManager.GameClient.DisconnectOpcode)
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
                WebClient.Client.CancelPendingRequests();
                NetworkManager.CancelClientConnectingTokenSource();
            });
        }

        public static async Task JoinServer(LobbyListing info)
        {
            if (SceneGameServers.ConnectingToLobby)
                return;

            SceneLobby.CurrentLobby = info;
            SceneGameServers.ConnectingToLobby = true;

            GD.Print("Connecting to lobby...");
            NetworkManager.StartClient(info.Ip, info.Port);

            await NetworkManager.WaitForClientToConnect(3000, async () =>
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
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

        public static CancellationTokenSource PingServersCTS;

        public async Task ListServers()
        {
            SceneGameServers.GettingServers = true;
            UIGameServersNavBtns.BtnRefresh.Disabled = true;

            WebClient.TaskGetServers = WebClient.Get<LobbyListing[]>("servers/get");
            var res = await WebClient.TaskGetServers;

            if (res.Status == WebServerStatus.ERROR)
                return;

            LobbyListings.Clear();

            var tasks = new List<Task>();

            res.Content.ForEach(async server =>
            {
                PingServersCTS = new CancellationTokenSource();
                PingServersCTS.CancelAfter(1000);

                var dummyClient = new ENetClient();
                dummyClient.Start("127.0.0.1", 7777);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        while (!dummyClient.IsConnected)
                            await Task.Delay(100, PingServersCTS.Token);

                        await dummyClient.Send(Netcode.ClientPacketOpcode.Ping);

                        while (!dummyClient.WasPingReceived)
                            await Task.Delay(1, PingServersCTS.Token);

                        dummyClient.WasPingReceived = false;
                    }
                    catch (TaskCanceledException) { }
                }, PingServersCTS.Token);

                tasks.Add(task);

                await task;

                if (!PingServersCTS.IsCancellationRequested)
                {
                    LobbyListings.Add(server.Ip, server);
                    server.Ping = dummyClient.PingMs;
                    AddServer(server);
                }

                dummyClient.Stop();
            });

            await Task.WhenAll(tasks);
            await Task.Delay(1000);

            SceneGameServers.GettingServers = false;

            if (SceneManager.InGameServers())
                UIGameServersNavBtns.BtnRefresh.Disabled = false;
        }

        public async void PostServer(LobbyListing info) =>
            await WebClient.Post("servers/post", new Dictionary<string, string>
            {
                { "Name", info.Name },
                { "Ip", info.Ip },
                { "Port", "" + info.Port },
                { "Description", info.Description },
                { "MaxPlayerCount", "" + info.MaxPlayerCount },
                { "LobbyHost", info.LobbyHost }
            });

        private void _on_Control_resized()
        {
            if (ServerCreationPopup != null)
                ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }
    }
}