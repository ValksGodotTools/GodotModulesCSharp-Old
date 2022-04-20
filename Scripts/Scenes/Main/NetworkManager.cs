using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using GodotModules.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public override void _Ready()
        {
            Instance = this;
            WebClient = new WebClient();
            GodotCmds = new ConcurrentQueue<GodotCmd>();
        }

        public override void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.PopupMessage:
                        GameManager.SpawnPopupMessage((string)cmd.Data);
                        break;
                    case GodotOpcode.LogMessageServer:
                        Utils.Log($"[Server]: {cmd.Data}", ENetServer.LogsColor);
                        break;
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        Utils.Log($"Received new server packet: {opcode}", ENetClient.LogsColor);

                        ENetClient.HandlePacket[opcode].Handle(packetReader);

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessageClient:
                        Utils.Log($"[Client]: {cmd.Data}", ENetClient.LogsColor);
                        break;
                    case GodotOpcode.ChangeScene:
                        SceneManager.ChangeScene($"{cmd.Data}");
                        break;
                    case GodotOpcode.PopupError:
                        GameManager.SpawnPopupError((Exception)cmd.Data);
                        break;
                }
            }
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Instance.GetTree().SetAutoAcceptQuit(false);

                if (ENetClient.Running)
                {
                    await GameClient.Stop();
                }

                if (ENetServer.Running) 
                {
                    GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                    await GameServer.Stop();
                }

                ExitCleanup();
            }
        }

        /// <summary>
        /// All cleanup should be done in here
        /// </summary>
        private static void ExitCleanup()
        {
            UtilOptions.SaveOptions();
            WebClient.Client.Dispose();

            if (ENetClient.CancelTokenSource != null)
                ENetClient.CancelTokenSource.Dispose();
            if (ENetServer.CancelTokenSource != null)
                ENetServer.CancelTokenSource.Dispose();

            Instance.GetTree().Quit();
        }

        public static void StartClient(string ip, ushort port)
        {
            GameClient = new GameClient();
            GameClient.Connect(ip, port);
        }

        public static void StartServer(ushort port, int maxClients)
        {
            GameServer = new GameServer();
            GameServer.Start(port, maxClients);
        }

        public static async Task WaitForHostToConnectToServer()
        {
            while (!ENetServer.SomeoneConnected)
                await Task.Delay(200);
        }

        public static async Task ClientConnecting()
        {
            while (!ENetClient.Connected)
                await Task.Delay(200);
        }
    }
}