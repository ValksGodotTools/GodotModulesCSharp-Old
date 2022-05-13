using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public Node ActiveScene { get; set; }

        private Scene _curScene { get; set; }
        private Scene _prevScene { get; set; }

        private readonly Dictionary<Scene, PackedScene> _scenes = new Dictionary<Scene, PackedScene>();
        private readonly Dictionary<Scene, Action<Node>> _preInit = new Dictionary<Scene, Action<Node>>();
        private readonly Game _game;
        private readonly GodotFileManager _godotFileManager;
        private readonly HotkeyManager _hotkeyManager;

        public SceneManager(Game game, GodotFileManager godotFileManager, HotkeyManager hotkeyManager) 
        {
            _game = game;
            _godotFileManager = godotFileManager;
            _hotkeyManager = hotkeyManager;
        }

        public async Task InitAsync()
        {
            var loadedScenes = _godotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                await ChangeScene(Scene.Menu);
        }

        public void PreInit(Scene scene, Action<Node> codeBeforeSceneReady)
        {
            _preInit[scene] = (scene) => {
                codeBeforeSceneReady(scene);
            };
        }

        public async Task ChangeScene(Scene scene, bool instant = true)
        {
            if (_curScene == scene)
                return;
                
            _prevScene = _curScene;
            _curScene = scene;

            if (_game.GetChildCount() != 0) 
                _game.GetChild(0).QueueFree();

            if (!instant)
                await Task.Delay(1);

            ActiveScene = _scenes[scene].Instance();

            if (_preInit.ContainsKey(scene))
                _preInit[scene](ActiveScene);

            _game.AddChild(ActiveScene);
        }

        public async Task IfEscGoToPrevScene() 
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                await ChangeScene(_prevScene);
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

        private void LoadScene(string scene) => _scenes[(Scene)Enum.Parse(typeof(Scene), scene)] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }

    public enum Scene
    {
        Game,
        GameServers,
        Lobby,
        Menu,
        Options,
        Mods,
        Credits
    }
}