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
        public static WebClient WebClient { get; set; }
        public static NetworkManager Instance { get; set; }

        public static DisconnectOpcode DisconnectOpcode { get; set; }
        public static uint PeerId { get; set; } // this clients peer id (grabbed from server at some point)
        public static bool IsHost { get; set; }

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

                        if (!string.IsNullOrWhiteSpace(message.Path))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            GD.Print($"   at ({message.Path})");
                        }

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

                    case GodotOpcode.Disconnect:
                        DisconnectOpcode = (DisconnectOpcode)cmd.Data;
                        await SceneManager.ChangeScene("GameServers");
                        break;
                }
            }
        }

        public static void StartClient(string ip, ushort port)
        {
            GameClient = new GameClient();
            GameClient.Start(ip, port);
        }

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

    public struct GodotMessage
    {
        public string Text { get; set; }
        public string Path { get; set; }
        public ConsoleColor Color { get; set; }
    }

    public struct GodotError
    {
        public Exception Exception { get; set; }
        public string Hint { get; set; }
    }
}