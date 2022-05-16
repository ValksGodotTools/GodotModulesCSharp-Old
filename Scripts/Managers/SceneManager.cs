using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public readonly Dictionary<GameScene, Action> EscPressed = new Dictionary<GameScene, Action>();

        public GameScene CurScene { get; set; }
        public GameScene PrevScene { get; set; }

        private AScene _activeScene;
        private readonly Dictionary<GameScene, PackedScene> _scenes = new Dictionary<GameScene, PackedScene>();
        private readonly GodotFileManager _godotFileManager;
        private readonly HotkeyManager _hotkeyManager;
        private readonly Control _sceneList;
        private Managers _managers;

        public SceneManager(Control sceneList, GodotFileManager godotFileManager, HotkeyManager hotkeyManager, Managers managers) 
        {
            _sceneList = sceneList;
            _godotFileManager = godotFileManager;
            _hotkeyManager = hotkeyManager;
            _managers = managers;
        }

        public async Task InitAsync()
        {
            var loadedScenes = _godotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
            {
                if (!dir.CurrentIsDir())
                    LoadScene(fileName.Replace(".tscn", ""));
            });

            if (loadedScenes)
                await ChangeScene(GameScene.Menu);
        }

        public async Task ChangeScene(GameScene scene, bool instant = true)
        {
            if (CurScene == scene)
                return;
                
            PrevScene = CurScene;
            CurScene = scene;

            if (_sceneList.GetChildCount() != 0) 
                _sceneList.GetChild(0).QueueFree();

            if (!instant)
                await Task.Delay(1);

            _activeScene = (AScene)_scenes[scene].Instance();
            _activeScene.PreInit(_managers);

            _sceneList.AddChild(_activeScene);
        }

        private void LoadScene(string scene) => _scenes[(GameScene)Enum.Parse(typeof(GameScene), scene)] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
    }

    public enum GameScene
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