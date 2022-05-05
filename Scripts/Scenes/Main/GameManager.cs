using Godot;
using System;
using System.Threading.Tasks;

namespace GodotModules
{
    // GameManager is attached to Global.tscn which is in AutoLoad
    public class GameManager : Node
    {
        public static string GameName = "Godot Modules";
        public static OptionsData Options { get; set; }
        public static GameManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_debug"))
            {
                UIDebugger.ToggleVisibility();
            }

            if (Input.IsActionJustPressed("ui_fullscreen"))
                UtilOptions.ToggleFullscreen();
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
            try
            {
                if (NetworkManager.GameServer != null)
                    if (NetworkManager.GameServer.IsRunning)
                    {
                        NetworkManager.GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                        NetworkManager.GameServer.Dispose();
                        NetworkManager.GameServer.Stop();

                        while (NetworkManager.GameServer.IsRunning)
                            await Task.Delay(100);
                    }

                if (NetworkManager.GameClient != null)
                    if (NetworkManager.GameClient.IsRunning)
                    {
                        NetworkManager.GameClient.Dispose();
                        NetworkManager.GameClient.Stop();

                        while (NetworkManager.GameClient.IsRunning)
                            await Task.Delay(100);
                    }

                UtilOptions.SaveOptions();
                NetworkManager.WebClient.Dispose();
            }
            catch (Exception e)
            {
                Logger.LogErr(e, "Game exit cleanup");
                await Task.Delay(3000);
            }

            Instance.GetTree().Quit();
        }

        public static void SpawnPopupMessage(string message)
        {
            var popupMessage = Prefabs.PopupMessage.Instance<UIPopupMessage>();
            Instance.GetTree().CurrentScene.AddChild(popupMessage);
            popupMessage.Init(message);
            popupMessage.PopupCentered();
        }

        public static void SpawnPopupError(Exception e)
        {
            ErrorNotifier.IncrementErrorCount();
            var popupError = Prefabs.PopupError.Instance<UIPopupError>();
            Instance.GetTree().CurrentScene.AddChild(popupError);
            popupError.Init(e.Message, e.StackTrace);
            popupError.PopupCentered();
        }

        /// <summary>
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => Instance.GetTree().Notification(MainLoop.NotificationWmQuitRequest);
    }
}