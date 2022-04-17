using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using GodotModules.Settings;
using System.Collections.Generic;

namespace GodotModules
{
    // GameManager is attached to Global.tscn which is in AutoLoad
    public class GameManager : Node
    {
        public static string GameName { get; set; }
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }
        public static OptionsData Options { get; set; }
        private static GameManager Instance { get; set; }
        public static string ActiveScene { get; set; }

        public override void _Ready()
        {
            GameName = "Godot Modules";
            ActiveScene = "Menu";
            Instance = this;
            UtilOptions.InitOptions();
            InitClient();
            InitServer();
            InitWebClient();
        }

        public override void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                Instance.GetTree().SetAutoAcceptQuit(false); // not sure if this is required
                ExitCleanup();
            }
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
            WebClient.Client.CancelPendingRequests();
            WebClient.Client.Dispose();

            Instance.GetTree().Quit();
        }

        private static void InitClient()
        {
            GameClient = new GameClient();
            GameClient.Name = "Game Client";
            Instance.AddChild(GameClient);
        }

        private static void InitServer()
        {
            GameServer = new GameServer();
            GameServer.Name = "Game Server";
            Instance.AddChild(GameServer);
        }

        private static void InitWebClient()
        {
            WebClient = new WebClient();
            WebClient.Name = "Web Client";
            Instance.AddChild(WebClient);
        }
    }
}