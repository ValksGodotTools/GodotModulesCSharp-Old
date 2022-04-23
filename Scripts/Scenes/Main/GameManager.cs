using Thread = System.Threading.Thread;

using Godot;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using GodotModules.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GodotModules
{
    // GameManager is attached to Global.tscn which is in AutoLoad
    public class GameManager : Node
    {
        public static PackedScene PrefabPopupMessage = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupMessage.tscn");
        public static PackedScene PrefabPopupError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/PopupError.tscn");
        public static PackedScene PrefabNotifyError = ResourceLoader.Load<PackedScene>("res://Scenes/Prefabs/NotifyError.tscn");
        public static string GameName = "Godot Modules";
        public static OptionsData Options { get; set; }
        public static GameManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            ErrorNotifier.StartInterval(2);
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_debug"))
            {
                UIDebugger.ToggleVisibility();
                ErrorNotifier.IncrementErrorCount();
            }

            if (Input.IsActionJustPressed("ui_fullscreen"))
                UtilOptions.ToggleFullscreen();
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
            ErrorNotifier.IncrementErrorCount();

            var popupError = GameManager.PrefabPopupError.Instance<UIPopupError>();
            popupError.Init(e.Message, e.StackTrace);
            Instance.GetTree().CurrentScene.AddChild(popupError);
            popupError.PopupCentered();
        }

        public static void NotifyError(int errorCount)
        {
            var notifyError = PrefabNotifyError.Instance<UINotifyError>();
            notifyError.Init(errorCount);
            Instance.CallDeferred("add_child", notifyError);
        }

        /// <summary>
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => Instance.GetTree().Notification(MainLoop.NotificationWmQuitRequest);
    }
}