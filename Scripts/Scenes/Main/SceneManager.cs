using Godot;
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

            LoadAllScenes();

            ChangeScene("Menu");
        }

        public static void ChangeScene(string scene)
        {
            ActiveScene = scene;
            if (Instance.GetChildCount() != 0)
                Instance.GetChild(0).QueueFree();
            Instance.AddChild(Scenes[scene].Instance());
        }

        private void LoadAllScenes()
        {
            var dir = new Godot.Directory();
            var path = "res://Scenes/Scenes";
            var error = dir.Open(path);

            if (error != Error.Ok)
                GD.PrintErr($"Failed to open {path}");

            dir.ListDirBegin(true);
            var fileName = dir.GetNext();
            while (fileName != "") 
            {
                if (!dir.CurrentIsDir()) 
                {
                    LoadScene(fileName.Replace(".tscn", ""));
                }

                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }

        private void LoadScene(string scene) => Scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}