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
        public UIPopupCreateLobby ServerCreationPopup { get; set; }
        public UILobbyListing SelectedLobbyInstance { get; set; }
        public bool GettingServers { get; set; }

        [Export] public readonly NodePath NodePathServerList;
        [Export] public readonly NodePath NodePathServerCreationPopup;

        private Dictionary<string, LobbyListing> LobbyListings { get; set; }
        private VBoxContainer ServerList { get; set; }
        private CancellationTokenSource CTSClientConnecting { get; set; }
        private CancellationTokenSource CTSPingServers { get; set; }

        public override async void _Ready()
        {
            CTSClientConnecting = new CancellationTokenSource();
            GettingServers = true; // because we await GetServers() at bottom
            UIGameServersNavBtns.BtnRefresh.Disabled = true;
            ServerList = GetNode<VBoxContainer>(NodePathServerList);
            ServerCreationPopup = GetNode<UIPopupCreateLobby>(NodePathServerCreationPopup);

            LobbyListings = new();

            if (GameClient.Disconnected)
            {
                GameClient.Disconnected = false;
                var message = "Disconnected";

                switch (NetworkManager.DisconnectOpcode)
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

            await WebClient.UpdateIsAlive();

            if (WebClient.ConnectionAlive)
                await ListServers();
        }

        public override void _Input(InputEvent @event)
        {
            SceneManager.EscapePressed(async () =>
            {
                WebClient.Client.CancelPendingRequests();

                if (NetworkManager.GameClient != null)
                    NetworkManager.GameClient.CancelTask();

                await SceneManager.ChangeScene("Menu");
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

            await ClientConnect(async () =>
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby
                {
                    LobbyOpcode = LobbyOpcode.LobbyJoin,
                    Username = GameManager.Options.OnlineUsername,
                    DirectConnect = directConnect
                });
            });
        }

        public async Task ClientConnect(Action action) => await NetworkManager.WaitForClientToConnect(3000, CTSClientConnecting, action);

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
                CTSPingServers = new CancellationTokenSource();
                CTSPingServers.CancelAfter(1000);

                var dummyClient = new ENetClient();
                dummyClient.Start(server.Ip, server.Port);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        while (!dummyClient.IsConnected)
                            await Task.Delay(100, CTSPingServers.Token);

                        await dummyClient.Send(Netcode.ClientPacketOpcode.Ping);

                        while (!dummyClient.WasPingReceived)
                            await Task.Delay(1, CTSPingServers.Token);

                        dummyClient.WasPingReceived = false;
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.LogDebug("Dummy client task cancelled (A)");
                    }
                }, CTSPingServers.Token);

                tasks.Add(task);

                try 
                {
                    await task;
                } catch(TaskCanceledException)
                {
                    Logger.LogDebug("Dummy client task cancelled (B)");
                }



                if (!CTSPingServers.IsCancellationRequested)
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

        public async void PostServer(LobbyListing info)
        {
            await WebClient.Post("servers/add", new Dictionary<string, string>
            {
                { "Name", info.Name },
                { "Ip", WebClient.ExternalIp },
                { "Port", "" + info.Port },
                { "Description", info.Description },
                { "MaxPlayerCount", "" + info.MaxPlayerCount },
                { "LobbyHost", info.LobbyHost }
            });
        }

        private void _on_Control_resized()
        {
            //if (ServerCreationPopup != null)
            //ServerCreationPopup.RectGlobalPosition = RectSize / 2 - ServerCreationPopup.RectSize / 2;
        }

        public override void Cleanup()
        {
            if (CTSPingServers != null)
            {
                CTSPingServers.Cancel();
                CTSPingServers.Dispose();
            }

            CTSClientConnecting.Cancel();
            CTSClientConnecting.Dispose();
        }
    }
}