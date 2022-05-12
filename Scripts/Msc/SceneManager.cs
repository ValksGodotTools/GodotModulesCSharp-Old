using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public string PrevSceneName { get; set; }
        public string CurSceneName { get; set; }
        public Node ActiveScene { get; set; }

        private Dictionary<string, PackedScene> Scenes = new Dictionary<string, PackedScene>();

        private Game _game;
        private GodotFileManager _godotFileManager;

        public SceneManager(Game game, GodotFileManager godotFileManager) 
        {
            _game = game;
            _godotFileManager = godotFileManager;
        }

        public async Task InitAsync()
        {
            var loadedScenes = _godotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
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
            if (CurSceneName == sceneName || string.IsNullOrWhiteSpace(sceneName))
                return;
                
            PrevSceneName = CurSceneName;
            CurSceneName = sceneName;

            if (_game.GetChildCount() != 0) 
            {
                var scene = _game.GetChild(0);
                scene.QueueFree();
            }

            if (!instant)
                await Task.Delay(1);

            ActiveScene = Scenes[sceneName].Instance();
            _game.AddChild(ActiveScene);
        }

        public void IfEscapePressed(Action code)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                /*if (GM.GameConsoleVisible) 
                {
                    GM.ToggleConsoleVisibility();
                    return;
                }*/

                code();
            }
        }

        private void LoadScene(string scene) => Scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}