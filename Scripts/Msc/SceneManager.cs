using Godot;
using System;

namespace GodotModules
{
    public class SceneManager
    {
        public readonly Dictionary<Scene, Action> EscPressed = new Dictionary<Scene, Action>();
        public readonly Dictionary<Scene, Action<Node>> PreInit = new Dictionary<Scene, Action<Node>>();
        public Node ActiveScene { get; set; }

        public Scene CurScene { get; set; }
        public Scene PrevScene { get; set; }

        private readonly Dictionary<Scene, PackedScene> _scenes = new Dictionary<Scene, PackedScene>();
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

            ActiveScene = _scenes[scene].Instance();

            if (PreInit.ContainsKey(scene))
                PreInit[scene](ActiveScene);

            _game.AddChild(ActiveScene);
        }

        public async Task IfEscGoToPrevScene() 
        {
            if (Input.IsActionJustPressed("ui_cancel"))
                await ChangeScene(PrevScene);
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