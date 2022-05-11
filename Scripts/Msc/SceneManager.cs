using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        private Dictionary<string, PackedScene> Scenes = new Dictionary<string, PackedScene>();
        public string PrevSceneName { get; set; }
        public string CurSceneName { get; set; }
        public Node ActiveScene { get; set; }

        public async Task Init()
        {
            var loadedScenes = GM.LoadDirectory("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                await ChangeScene("Menu");
        }

        public bool InMainMenu() => CurSceneName == "Menu";
        public bool InGameServers() => CurSceneName == "GameServers";
        public bool InLobby() => CurSceneName == "Lobby";
        public bool InGame() => CurSceneName == "Game";

        public async Task ChangeScene(string sceneName, bool instant = true)
        {
            if (CurSceneName == sceneName)
                return;
                
            PrevSceneName = CurSceneName;
            CurSceneName = sceneName;

            if (GM.Instance.GetChildCount() != 0) 
            {
                var scene = GM.Instance.GetChild(0);
                scene.QueueFree();
            }

            if (!instant)
                await Task.Delay(1);

            ActiveScene = Scenes[sceneName].Instance();
            GM.Instance.AddChild(ActiveScene);
        }

        public void EscapePressed(Action action)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                /*if (GM.GameConsoleVisible) 
                {
                    GM.ToggleConsoleVisibility();
                    return;
                }*/

                action();
            }
        }

        private void LoadScene(string scene) => Scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}