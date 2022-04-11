using Godot;
using System;
using Valk.Modules.Settings;

namespace Valk.Modules 
{
    public class GameManager : Node
    {
        private static GameManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;

            if (!System.IO.File.Exists(UIOptions.PathOptions))
                FileManager.WriteConfig<Options>(UIOptions.PathOptions);
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
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => Instance.GetTree().Notification(MainLoop.NotificationWmQuitRequest);

        private static void ExitCleanup()
        {
            UIOptions.SaveOptions();

            Instance.GetTree().Quit();
        }
    }
}
