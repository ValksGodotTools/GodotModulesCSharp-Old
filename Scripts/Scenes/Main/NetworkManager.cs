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

        public override async void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetHandleData = (PacketHandleData)cmd.Data;
                        var packetReader = packetHandleData.Reader;
                        var client = packetHandleData.Client;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //Utils.Log($"[Client]: Received {opcode}");

                        if (!ENetClient.HandlePacket.ContainsKey(opcode))
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException ex)
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} {ex.Message} (Ignoring)");
                            break;
                        }
                        await handlePacket.Handle(client);

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessage:
                        var message = (GodotMessage)cmd.Data;
                        var text = message.Text;
                        var color = message.Color;

                        Console.ForegroundColor = color;
                        GD.Print(text);
                        Console.ResetColor();

                        UIDebugger.AddMessage(text);
                        break;

                    case GodotOpcode.LogError:
                        var error = (GodotError)cmd.Data;
                        var exception = error.Exception;
                        var hint = error.Hint;

                        var errorText = $"[Error]: {hint}{exception.Message}\n{exception.StackTrace}";

                        Console.ForegroundColor = ConsoleColor.Red;
                        GD.PrintErr(exception);
                        Console.ResetColor();
                        
                        ErrorNotifier.IncrementErrorCount();
                        UIDebugger.AddMessage(errorText);
                        break;

                    case GodotOpcode.PopupMessage:
                        GameManager.SpawnPopupMessage((string)cmd.Data);
                        break;

                    case GodotOpcode.ChangeScene:
                        await SceneManager.ChangeScene($"{cmd.Data}");
                        break;
                }
            }
        }

        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Instance.GetTree().SetAutoAcceptQuit(false);

                ExitCleanup();
            }
        }

        /// <summary>
        /// All cleanup should be done in here
        /// </summary>
        private static void ExitCleanup()
        {
            try
            {
                if (NetworkManager.GameServer != null)
                    if (NetworkManager.GameServer.Running)
                    {
                        GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                        GameServer.Stop();
                        GameServer.Dispose();
                    }

                if (NetworkManager.GameClient != null)
                    if (NetworkManager.GameClient.Running) // THREAD SAFETY VIOLATION
                    {
                        GameClient.Stop();
                        GameClient.Dispose();
                    }

                UtilOptions.SaveOptions();
                WebClient.Dispose();

                //if (SceneGameServers.PingServersCTS != null)
                //SceneGameServers.PingServersCTS.Dispose();
                if (ClientConnectingTokenSource != null)
                    ClientConnectingTokenSource.Dispose();
            }
            catch (Exception e)
            {
                GD.Print("Exception on game exit cleanup: " + e);
            }

            Instance.GetTree().Quit();
        }

        public static void StartClient(string ip, ushort port)
        {
            if (GameClient != null)
                GameClient.Dispose();
            GameClient = new GameClient();
            GameClient.Start(ip, port);
        }

        public static async void StartServer(ushort port, int maxClients)
        {
            if (GameServer != null)
                GameServer.Dispose();
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
                while (!NetworkManager.GameClient.IsConnected) // THREAD SAFETY VIOLATION
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

            ClientConnectingTokenSource.Cancel();
            GameClient.CancelTask(); // THREAD SAFETY VIOLATION
        }
    }

    public struct GodotMessage
    {
        public string Text { get; set; }
        public ConsoleColor Color { get; set; }
    }

    public struct GodotError
    {
        public Exception Exception { get; set; }
        public string Hint { get; set; }
    }
}