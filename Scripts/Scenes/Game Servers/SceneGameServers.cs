using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    public class SceneGameServers : AScene
    {
        public Dictionary<string, LobbyListing> LobbyListings { get; set; }
        public UILobbyListing SelectedLobbyInstance { get; set; }
        public bool GettingServers { get; set; }

        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        private VBoxContainer ServerList { get; set; }
        public UIPopupCreateLobby ServerCreationPopup { get; set; }

        public override async void _Ready()
        {
            GettingServers = true; // because we await GetServers() at bottom
            UIGameServersNavBtns.BtnRefresh.Disabled = true;

            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UIPopupCreateLobby>(NodePathServerCreationPopup);
            LobbyListings = new();

            if (GameClient.Disconnected)
            {
                GameClient.Disconnected = false;
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

        public override async void _Input(InputEvent @event)
        {
            await SceneManager.EscapeToScene("Menu", () =>
            {
                WebClient.Client.CancelPendingRequests();
                NetworkManager.CancelClientConnectingTokenSource();
            });
        }

        public async Task JoinServer(LobbyListing info, bool directConnect)
        {
            if (GameClient.ConnectingToLobby)
                return;

            SceneLobby.CurrentLobby = info;
            GameClient.ConnectingToLobby = true;

            GD.Print("Connecting to lobby...");
            NetworkManager.StartClient(info.Ip, info.Port);

            await NetworkManager.WaitForClientToConnect(3000, async () =>
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyJoin,
                    Username = GameManager.Options.OnlineUsername,
                    DirectConnect = directConnect
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

        public CancellationTokenSource PingServersCTS;

        public async Task ListServers()
        {
            GettingServers = true;
            UIGameServersNavBtns.BtnRefresh.Disabled = true;

            WebClient.TaskGetServers = WebClient.Get<LobbyListing[]>("servers/get");
            var res = await WebClient.TaskGetServers;

            if (!SceneManager.InGameServers())
                return;

            if (res.Status == WebServerStatus.ERROR)
            {
                GettingServers = false;
                UIGameServersNavBtns.BtnRefresh.Disabled = false;
                return;
            }

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
                    LobbyListings[server.Ip] = server;
                    server.Ping = dummyClient.PingMs;
                    AddServer(server);
                }

                dummyClient.Stop();
            });

            await Task.WhenAll(tasks);
            await Task.Delay(1000);

            GettingServers = false;

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
            //if (ServerCreationPopup != null)
            //ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }

        public override void Cleanup()
        {
            if (PingServersCTS != null)
            {
                PingServersCTS.Cancel();
                PingServersCTS.Dispose();
            }
        }
    }
}