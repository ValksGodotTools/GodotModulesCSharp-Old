using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public readonly Dictionary<Scene, Action> EscPressed = new Dictionary<Scene, Action>();
        // public readonly Dictionary<Scene, Action<Node>> PreInit = new Dictionary<Scene, Action<Node>>();

        public Scene CurScene { get; set; }
        public Scene PrevScene { get; set; }

        private Node _activeScene;
        private readonly Dictionary<Scene, PackedScene> _scenes = new Dictionary<Scene, PackedScene>();
        private readonly Game _game;

        [Inject] private GodotFileManager _godotFileManager;
        [Inject] private HotkeyManager _hotkeyManager;

        public SceneManager(Game game) 
        {
            _game = game;
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

        public async Task ChangeScene(Scene scene, bool instant = true)
        {
            if (CurScene == scene)
                return;
                
            PrevScene = CurScene;
            CurScene = scene;

            if (_game.GetChildCount() != 0) 
                _game.GetChild(0).QueueFree();

            if (!instant)
                await Task.Delay(1);

            _activeScene = Container.Inject(_scenes[scene].Instance());
            _game.AddChild(_activeScene);
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