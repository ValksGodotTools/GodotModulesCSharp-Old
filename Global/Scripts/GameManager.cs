using Common.Netcode;
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
    // GameManager is attached to Global.tscn which is in AutoLoad
    public class GameManager : Node
    {
        public static PackedScene PrefabPopupMessage = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupMessage.tscn");
        public static PackedScene PrefabPopupError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupError.tscn");
        public static string GameName { get; set; }
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }
        public static OptionsData Options { get; set; }
        public static GameManager Instance { get; set; }
        public static string ActiveScene { get; set; }

        public override void _Ready()
        {
            GameName = "Godot Modules";
            ActiveScene = "Menu";
            Instance = this;
            UtilOptions.InitOptions();
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

        public static void SpawnPopupMessage(string message)
        {
            var popupMessage = GameManager.PrefabPopupMessage.Instance<UIPopupMessage>();
            popupMessage.Init(message);
            Instance.GetTree().CurrentScene.AddChild(popupMessage);
            popupMessage.PopupCentered();
        }

        public static void SpawnPopupError(Exception e)
        {
            var popupError = GameManager.PrefabPopupError.Instance<UIPopupError>();
            popupError.Init(e.Message, e.StackTrace);
            Instance.GetTree().CurrentScene.AddChild(popupError);
            popupError.PopupCentered();
        }

        /// <summary>
        /// This should be used over GetTree().ChangeScene(string path)
        /// </summary>
        /// <param name="scene"></param>
        public static void ChangeScene(string scene)
        {
            ActiveScene = scene;
            Instance.GetTree().ChangeScene($"res://Scenes/{scene}.tscn");
        }

        /// <summary>
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => Instance.GetTree().Notification(MainLoop.NotificationWmQuitRequest);

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

        public static async Task ServerAndClientReady()
        {
            if (!ENetClient.Connected && !ENetServer.SomeoneConnected)
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
                        ChangeScene($"{cmd.Data}");
                        break;
                    case GodotOpcode.PopupError:
                        SpawnPopupError((Exception)cmd.Data);
                        break;
                }
            }
        }
    }
}