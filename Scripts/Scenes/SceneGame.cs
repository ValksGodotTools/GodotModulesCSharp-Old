using Godot;

namespace GodotModules 
{
    public class SceneGame : AScene
    {
        private GameManager _gameData;

        public override void PreInit(Managers managers)
        {
            
        }

        public override void _Ready()
        {
            _gameData = new GameManager(this);
            _gameData.CreateMainPlayer();
            _gameData.CreateEnemy(new Vector2(200, 200));
        }
    }

    public class GameManager
    {
        public List<OtherPlayer> Players { get; set; }
        public Player Player { get; set; }
        public List<Enemy> Enemies { get; set; }

        private Node _sceneGame;

        public GameManager(Node sceneGame)
        {
            _sceneGame = sceneGame;
            Enemies = new();
            Players = new();
        }

        public void CreateMainPlayer()
        {
            var player = Prefabs.Player.Instance<Player>();
            _sceneGame.AddChild(player);
            Player = player;
            Players.Add(player);
        }

        public void CreateOtherPlayer()
        {
            var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
            _sceneGame.AddChild(otherPlayer);
            Players.Add(otherPlayer);
        }

        public void CreateEnemy(Vector2 position)
        {
            var enemy = Prefabs.Enemy.Instance<Enemy>();
            enemy.Init(Players);
            enemy.Position = position;
            _sceneGame.AddChild(enemy);
            Enemies.Add(enemy);
        }
    }
}
