using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    public class NetworkManager : Node
    {
        public static ConcurrentQueue<GodotCmd> GodotCmds { get; set; }
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        private static WebClient WebClient { get; set; }
        public static NetworkManager Instance { get; set; }
        private static CancellationTokenSource ClientConnectingTokenSource { get; set; }

        public override void _Ready()
        {
            Instance = this;
            GodotCmds = new();
            WebClient = new();
        }

        public override void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //Utils.Log($"[Client]: Received {opcode}");

                        if (!ENetClient.HandlePacket.ContainsKey(opcode))
                        {
                            Utils.LogErr($"[Client]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException)
                        {
                            Utils.LogErr($"[Client]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }
                        handlePacket.Handle();

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessageServer:
                        Utils.Log($"[Server]: {cmd.Data}", ENetServer.LogsColor);
                        break;

                    case GodotOpcode.LogMessageClient:
                        Utils.Log($"[Client]: {cmd.Data}", ENetClient.LogsColor);
                        break;

                    case GodotOpcode.Error:
                        var e = (Exception)cmd.Data;
                        Utils.LogErr(e);
                        GameManager.SpawnPopupError(e);
                        break;

                    case GodotOpcode.PopupMessage:
                        GameManager.SpawnPopupMessage((string)cmd.Data);
                        break;

                    case GodotOpcode.ChangeScene:
                        SceneManager.ChangeScene($"{cmd.Data}");
                        break;
                }
            }
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Instance.GetTree().SetAutoAcceptQuit(false);

                await ExitCleanup();
            }
        }

        /// <summary>
        /// All cleanup should be done in here
        /// </summary>
        private static async Task ExitCleanup()
        {
            if (NetworkManager.GameServer != null)
                if (NetworkManager.GameServer.Running)
                {
                    GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                    await GameServer.Stop();
                }

            if (NetworkManager.GameClient != null)
                if (NetworkManager.GameClient.Running)
                {
                    GameClient.Stop();
                    GameClient.Dispose();
                }

            UtilOptions.SaveOptions();
            WebClient.Dispose();

            ClientConnectingTokenSource?.Dispose();

            Instance.GetTree().Quit();
        }

        public static void StartClient(string ip, ushort port)
        {
            GameClient?.Dispose();
            GameClient = new GameClient();
            GameClient.Start(ip, port);
        }

        public static async void StartServer(ushort port, int maxClients)
        {
            GameServer?.Dispose();
            GameServer = new GameServer();
            await GameServer.Start(port, maxClients);
        }

        public static async Task WaitForHostToConnectToServer()
        {
            while (!NetworkManager.GameServer.SomeoneConnected)
                await Task.Delay(200);
        }

        public static async Task WaitForClientToConnect(int timeoutMs, Action onClientConnected)
        {
            ClientConnectingTokenSource = new CancellationTokenSource();
            ClientConnectingTokenSource.CancelAfter(timeoutMs);
            await Task.Run(async () =>
            {
                while (!NetworkManager.GameClient.IsConnected)
                {
                    if (ClientConnectingTokenSource.IsCancellationRequested)
                        break;

                    await Task.Delay(100);
                }
            }, ClientConnectingTokenSource.Token).ContinueWith((task) =>
            {
                if (!ClientConnectingTokenSource.IsCancellationRequested)
                    onClientConnected();
            });
        }

        public static void CancelClientConnectingTokenSource()
        {
            if (ClientConnectingTokenSource == null)
                return;

            SceneGameServers.ConnectingToLobby = false;
            ClientConnectingTokenSource.Cancel();
            GameClient.CancelTask();
        }
    }
}