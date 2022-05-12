using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public Node ActiveScene { get; set; }

        private string _curSceneName { get; set; }
        private string _prevSceneName { get; set; }

        private readonly Dictionary<string, PackedScene> _scenes = new Dictionary<string, PackedScene>();
        private readonly Game _game;
        private readonly GodotFileManager _godotFileManager;

        public SceneManager(Game game, GodotFileManager godotFileManager) 
        {
            _game = game;
            _godotFileManager = godotFileManager;
        }

        public async Task InitAsync(HotkeyManager hotkeyManager)
        {
            var loadedScenes = _godotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                await ChangeScene("Menu", (scene) => {
                    var menu = (UIMenu)scene;
                    menu.HotkeyManager = hotkeyManager;
                });
        }

        public bool InMainMenu() => _curSceneName == "Menu";
        public bool InGameServers() => _curSceneName == "GameServers";
        public bool InLobby() => _curSceneName == "Lobby";
        public bool InGame() => _curSceneName == "Game";

        public async Task ChangeScene(string sceneName, Action<Node> setupBeforeReady = null, bool instant = true)
        {
            if (_curSceneName == sceneName || string.IsNullOrWhiteSpace(sceneName))
                return;
                
            _prevSceneName = _curSceneName;
            _curSceneName = sceneName;

            if (_game.GetChildCount() != 0) 
            {
                var scene = _game.GetChild(0);
                scene.QueueFree();
            }

            if (!instant)
                await Task.Delay(1);

            ActiveScene = _scenes[sceneName].Instance();

            if (setupBeforeReady != null)
                setupBeforeReady(ActiveScene);

            _game.AddChild(ActiveScene);
        }

        public async Task IfEscGoToPrevScene() 
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                await ChangeScene(_prevSceneName);
            }
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

        private void LoadScene(string scene) => _scenes[scene] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }
}