using Godot;
using System;
using System.Threading.Tasks;

namespace GodotModules
{
    public class SceneManager : Node
    {
        private static Dictionary<string, PackedScene> Scenes = new Dictionary<string, PackedScene>();
        public static string PrevSceneName { get; set; }
        public static string CurSceneName { get; set; }
        public static AScene ActiveScene { get; set; }
        public static SceneManager Instance { get; set; }

        public override async void _Ready()
        {
            Instance = this;
            UtilOptions.InitOptions();

            var loadedScenes = GodotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                await ChangeScene("Menu");
        }

        public static bool InMainMenu() => CurSceneName == "Menu";

        public static bool InGameServers() => CurSceneName == "GameServers";

        public static bool InLobby() => CurSceneName == "Lobby";

        public static bool InGame() => CurSceneName == "Game";

        public static async Task ChangeScene(string sceneName, bool instant = true)
        {
            if (CurSceneName == sceneName)
                return;
                
            PrevSceneName = CurSceneName;
            CurSceneName = sceneName;

            if (Instance.GetChildCount() != 0) 
            {
                var scene = (AScene)Instance.GetChild(0);
                scene.Cleanup();
                scene.QueueFree();
            }

            if (!instant)
                await Task.Delay(1);

            ActiveScene = (AScene)Scenes[sceneName].Instance();
            Instance.AddChild(ActiveScene);
        }

        public static void EscapePressed(Action action)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                if (GM.GameConsoleVisible) 
                {
                    GM.ToggleConsoleVisibility();
                    return;
                }

                action();
            }
        }

        public static T GetActiveSceneScript<T>() where T : AScene => (T)ActiveScene;

        private void LoadScene(string scene) => Scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}