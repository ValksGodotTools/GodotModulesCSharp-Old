using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using GodotModules.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodotModules 
{
    public class NetworkManager : Node 
    {
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }
        public static NetworkManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            InitWebClient();
        }

        public override void _Process(float delta)
        {
            ProcessENetServerGodotCmds();
            ProcessENetClientGodotCmds();
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Instance.GetTree().SetAutoAcceptQuit(false);

                if (ENetServer.Running) 
                    GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));

                if (ENetClient.Running)
                    GameClient.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));

                if (GameClient.WorkerClient != null && !GameClient.WorkerClient.IsCompleted)
                    await Task.Delay(100);

                if (GameServer.WorkerServer != null && !GameServer.WorkerServer.IsCompleted)
                    await Task.Delay(100);

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
                await Task.Delay(100);
        }

        public static async Task ClientConnecting()
        {
            while (!ENetClient.Connected)
                await Task.Delay(100);
        }

        private static void InitWebClient()
        {
            WebClient = new WebClient();
            WebClient.Name = "Web Client";
            Instance.AddChild(WebClient);
        }

        private void ProcessENetServerGodotCmds()
        {
            if (ENetServer.GodotCmds == null)
                return;

            while (ENetServer.GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.LogMessage:
                        Utils.Log($"[Server]: {cmd.Data}", ENetServer.LogsColor);
                        return;
                }
            }
        }

        private void ProcessENetClientGodotCmds()
        {
            if (ENetClient.GodotCmds == null)
                return;

            while (ENetClient.GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetReader = (PacketReader)cmd.Data;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        Utils.Log($"Received new server packet: {opcode}", ENetClient.LogsColor);

                        ENetClient.HandlePacket[opcode].Handle(packetReader);

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessage:
                        Utils.Log($"[Client]: {cmd.Data}", ENetClient.LogsColor);
                        break;
                    case GodotOpcode.ChangeScene:
                        GameManager.ChangeScene($"{cmd.Data}");
                        break;
                    case GodotOpcode.PopupError:
                        GameManager.SpawnPopupError((Exception)cmd.Data);
                        break;
                }
            }
        }
    }
}