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

            if (NetworkManager.ClientDisconnected)
            {
                NetworkManager.ClientDisconnected = false;
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

                GM.SpawnPopupMessage(message);
            }

            await NetworkManager.WebClient.UpdateIsAlive();

            if (NetworkManager.WebClient.ConnectionAlive)
                await ListServers();
        }

        public override void _Input(InputEvent @event)
        {
            SceneManager.EscapePressed(async () =>
            {
                NetworkManager.WebClient.Client.CancelPendingRequests();

                if (NetworkManager.GameClient != null)
                    NetworkManager.GameClient.Stop();

                await SceneManager.ChangeScene("Menu");
            });
        }

        public async Task JoinServer(LobbyListing info, bool directConnect)
        {
            if (NetworkManager.ClientConnectingToLobby)
                return;

            NetworkManager.CurrentLobby = info;
            NetworkManager.ClientConnectingToLobby = true;

            GM.Logger.Log("Connecting to lobby...");
            NetworkManager.StartClient(info.Ip, info.Port);

            await ClientConnect(async () =>
            {
                await NetworkManager.GameClient.Send(ClientPacketOpcode.Lobby, new CPacketLobby(LobbyOpcode.LobbyJoin)
                {
                    Username = GM.Options.OnlineUsername,
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

            var res = await NetworkManager.WebClient.TaskGetServers;

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

                        while (!NetworkManager.WasPingReceived)
                            await Task.Delay(1, CTSPingServers.Token);

                        NetworkManager.WasPingReceived = false;
                    }
                    catch (TaskCanceledException)
                    {
                        GM.Logger.LogDebug("Dummy client task cancelled (A)");
                    }
                }, CTSPingServers.Token);

                tasks.Add(task);

                try 
                {
                    await task;
                } catch(TaskCanceledException)
                {
                    GM.Logger.LogDebug("Dummy client task cancelled (B)");
                }

                if (!CTSPingServers.IsCancellationRequested)
                {
                    LobbyListings[server.Ip] = server;
                    server.Ping = NetworkManager.PingMs;
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
            await NetworkManager.WebClient.AddLobbyAsync(info);
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