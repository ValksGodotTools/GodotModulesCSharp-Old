using Godot;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    public class NetworkManager : Node
    {
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }

        public static DisconnectOpcode DisconnectOpcode { get; set; }
        public static uint PeerId { get; set; } // this clients peer id (grabbed from server at some point)
        public static bool IsHost { get; set; }

        // dummy client
        public static bool WasPingReceived { get; set; }
        public static DateTime PingSent { get; set; }
        public static int PingMs { get; set; }
        
        public static bool ServerAuthoritativeMovement { get; set; }

        public override void _Ready()
        {
            WebClient = new();
            ServerAuthoritativeMovement = false;
        }

        public static bool IsClientRunning() => GameClient == null ? false : GameClient.IsRunning;
        public static bool IsServerRunning() => GameServer == null ? false : GameServer.IsRunning;
        public static bool IsMultiplayer() => IsClientRunning() || IsServerRunning();

        public static void StartClient(string ip, ushort port)
        {
            GameClient = new GameClient();
            GameClient.Start(ip, port);
        }

        public static async void StartServer(ushort port, int maxClients)
        {
            GameServer = new GameServer();
            await GameServer.Start(port, maxClients);

            if (SceneLobby.CurrentLobby != null)
                if (SceneLobby.CurrentLobby.Public && WebClient.ConnectionAlive)
                    WebClient.TimerPingMasterServer.Start();
        }

        public static async Task WaitForHostToConnectToServer()
        {
            while (!NetworkManager.GameServer.HasSomeoneConnected)
                await Task.Delay(200);
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
    }
}