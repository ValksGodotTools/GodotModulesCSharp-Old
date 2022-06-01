namespace GodotModules
{
    public class SceneManager : Node
    {
        public readonly Dictionary<GameScene, Action<Node>> PreInit = new();

        public GameScene CurScene { get; private set; }
        public GameScene PrevScene { get; private set; }

        private Node _activeScene;
        private Dictionary<GameScene, PackedScene> _scenes = new();
        private GodotFileManager _godotFileManager;
        private HotkeyManager _hotkeyManager;
        private Control _sceneList;
        private Managers _managers;
        private GTimer _timer;

        public void Init(Control sceneList, HotkeyManager hotkeyManager, Managers managers) 
        {
            _sceneList = sceneList;
            _godotFileManager = new();
            _hotkeyManager = hotkeyManager;
            _managers = managers;

            _timer = new GTimer(this, 100);
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

            if (_timer.IsActive())
                return;

            _timer.Start();
                
            PrevScene = CurScene;
            CurScene = scene;

            if (_sceneList.GetChildCount() != 0) 
                _sceneList.GetChild(0).QueueFree();

            if (!instant)
                await Task.Delay(1);

            _activeScene = _scenes[scene].Instance();

            if (PreInit.ContainsKey(scene))
                PreInit[scene](_activeScene);

            if (_activeScene is AScene ascene)
                ascene.PreInitManagers(_managers);

            _sceneList.AddChild(_activeScene);

            await Task.Delay(1); // wait for listeners to become invalid

            Notifications.RemoveInvalidListeners();
            Notifications.Notify(this, Event.OnSceneChanged, scene);
        }

        public void HandleEscape(Action action)
        {
            if (Input.IsActionJustPressed("ui_cancel")) 
            {
                if (_managers.ManagerConsole.Visible)
                    _managers.ManagerConsole.ToggleVisibility();
                else
                    action();
            }
        }

        private void LoadScene(string scene) 
        {
            try 
            {
                _scenes[(GameScene)Enum.Parse(typeof(GameScene), scene)] = ResourceLoader.Load<PackedScene>($"res://Scenes/Scenes/{scene}.tscn");
            }
            catch (ArgumentException) 
            {
                Logger.LogWarning($"Enum for {scene} needs to be defined since the scene is in the Scenes directory");
            }
        }
    }

    public enum GameScene
    {
        Game,
        Game3D,
        GameServers,
        Lobby,
        Menu,
        Options,
        Mods,
        Credits
    }
}
