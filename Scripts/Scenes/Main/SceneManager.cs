using Godot;
using System;
using System.Collections.Generic;

namespace GodotModules
{
    public class SceneManager : Node
    {
        private static Dictionary<string, PackedScene> Scenes = new Dictionary<string, PackedScene>();
        public static string ActiveScene { get; set; }
        public static SceneManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            UtilOptions.InitOptions();

            var loadedScenes = GodotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                ChangeScene("Menu");
        }

        public static void ChangeScene(string scene)
        {
            ActiveScene = scene;
            if (Instance.GetChildCount() != 0)
                Instance.GetChild(0).QueueFree();
            Instance.AddChild(Scenes[scene].Instance());
        }

        private void LoadScene(string scene) => Scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}