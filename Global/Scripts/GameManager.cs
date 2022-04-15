using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using GodotModules.Settings;

namespace GodotModules
{
    // GameManager is attached to Global.tscn which is in AutoLoad
    public class GameManager : Node
    {
        public static string GameName { get; set; }
        public static GameServer GameServer { get; set; }
        public static GameClient GameClient { get; set; }
        public static WebClient WebClient { get; set; }
        private static GameManager Instance { get; set; }
        public static bool OptionsCreatedForFirstTime { get; set; }

        public override void _Ready()
        {
            GameName = "Godot Modules";
            Instance = this;
            InitOptions();
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

        private static void InitOptions()
        {
            if (!System.IO.File.Exists(UIOptions.PathOptions))
            {
                OptionsCreatedForFirstTime = true;
                FileManager.WriteConfig<Options>(UIOptions.PathOptions);
            }
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
            UIOptions.Instance.SaveOptions();
            WebClient.Client.CancelPendingRequests();
            WebClient.Client.Dispose();

            Instance.GetTree().Quit();
        }
    }
}