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
        public static OptionsData Options { get; set; }
        public static GameManager Instance { get; set; }
        public static string ActiveScene { get; set; }

        public override void _Ready()
        {
            GameName = "Godot Modules";
            ActiveScene = "Menu";
            Instance = this;
            UtilOptions.InitOptions();
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
    }
}