using Godot;
using GodotModules.Netcode.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

#if CLIENT
using GodotModules.Netcode.Client;
#endif

namespace GodotModules.Netcode
{
    public class NetworkManager : Node
    {
        private static SceneTree Tree {get; set; }

        public override void _Ready()
        {
            Tree = GetTree();
            ServerAuthoritativeMovement = false;

#if CLIENT
            WebClient = new();
#endif
        }

        // SERVER
        public static GameServer GameServer { get; set; }
        public static bool ServerAuthoritativeMovement { get; set; }
        private static int GameServerStillRunning { get; set; }
        public static bool IsServerRunning() => GameServer == null ? false : GameServer.IsRunning;
        public static async void StartServer(ushort port, int maxClients)
        {
            GameServer = new GameServer();
            await GameServer.Start(port, maxClients);
        }

        public static async Task WaitForHostToConnectToServer()
        {
            while (!NetworkManager.GameServer.HasSomeoneConnected)
                await Task.Delay(200);
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Tree.SetAutoAcceptQuit(false);

                await ExitCleanup();
            }
        }

        /// <summary>
        /// All cleanup should be done in here
        /// </summary>
        private static async Task ExitCleanup()
        {
            try
            {
                if (IsServerRunning())
                {
                    GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                    GameServer.Stop();

                    while (GameServer.IsRunning) 
                    {
                        GameServerStillRunning++;
                        if (GameServerStillRunning > 4)
                            Logger.LogDebug("Game server taking a long time to stop");
                        await Task.Delay(100);
                    }
                }

#if CLIENT
                if (IsClientRunning())
                {
                    GameClient.Stop();

                    while (GameClient.IsRunning) 
                    {
                        GameClientStillRunning++;
                        if (GameClientStillRunning > 4)
                            Logger.LogDebug("Game client taking a long time to stop");
                        await Task.Delay(100);
                    }
                }

                UtilOptions.SaveOptions();
                WebClient.Dispose();
#endif
            }
            catch (Exception e)
            {
                Logger.LogErr(e, "Game exit cleanup");
                await Task.Delay(3000);
            }

            Tree.Quit();
        }

#if CLIENT
        // CLIENT
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }

        public static DisconnectOpcode DisconnectOpcode { get; set; }
        public static uint PeerId { get; set; } // this clients peer id (grabbed from server at some point)
        public static bool IsHost { get; set; }

        // dummy client
        public static bool WasPingReceived { get; set; }
        public static DateTime PingSent { get; set; }
        public static int PingMs { get; set; }

        private static int GameClientStillRunning { get; set; }
        public static bool IsClientRunning() => GameClient == null ? false : GameClient.IsRunning;
        public static bool IsMultiplayer() => IsClientRunning() || IsServerRunning();

        public static void StartClient(string ip, ushort port)
        {
            GameClient = new GameClient();
            GameClient.Start(ip, port);
        }

        public static void BroadcastLobbyToMaster()
        {
            if (SceneLobby.CurrentLobby != null)
                if (SceneLobby.CurrentLobby.Public && WebClient.ConnectionAlive)
                    WebClient.TimerPingMasterServer.Start();
        }

        public static async Task WaitForClientToConnect(int timeoutMs, CancellationTokenSource cts, Action onClientConnected)
        {
            try
            {
                cts.CancelAfter(timeoutMs);
                await Task.Run(async () =>
                {
                    while (!NetworkManager.GameClient.IsConnected)
                    {
                        if (cts.IsCancellationRequested)
                            break;

                        await Task.Delay(100);
                    }
                }, cts.Token);

                if (!cts.IsCancellationRequested)
                    onClientConnected();
            }
            catch (Exception)
            { }
        }
#endif
    }
}