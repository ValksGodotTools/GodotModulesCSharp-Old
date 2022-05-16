using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public readonly Dictionary<GameScene, Action> EscPressed = new Dictionary<GameScene, Action>();
        public readonly Dictionary<GameScene, Action<Node>> PreInit = new Dictionary<GameScene, Action<Node>>();

        public GameScene CurScene { get; set; }
        public GameScene PrevScene { get; set; }

        private Node _activeScene;
        private readonly Dictionary<GameScene, PackedScene> _scenes = new Dictionary<GameScene, PackedScene>();
        private readonly GodotFileManager _godotFileManager;
        private readonly HotkeyManager _hotkeyManager;
        private readonly Control _sceneList;

        public SceneManager(Control sceneList, GodotFileManager godotFileManager, HotkeyManager hotkeyManager) 
        {
            _sceneList = sceneList;
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

            _activeScene = _scenes[scene].Instance();

            if (PreInit.ContainsKey(scene))
                PreInit[scene](_activeScene);

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