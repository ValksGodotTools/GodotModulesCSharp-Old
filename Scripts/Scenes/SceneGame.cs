using Godot;

namespace GodotModules 
{
    public class SceneGame : AScene
    {
        [Export] public readonly NodePath NodePathPositionPlayerSpawn;
        [Export] public readonly NodePath NodePathPositionEnemySpawn;
        [Export] public readonly NodePath NodePathNavigation2D;
        public Navigation2D Navigation2D { get; set; }
        public Position2D PositionPlayerSpawn { get; set; }
        public Position2D PositionEnemySpawn { get; set; }

        private GameManager _gameManager;

        public override void PreInitManagers(Managers managers)
        {
            
        }

        public override void _Ready()
        {
            Navigation2D = GetNode<Navigation2D>(NodePathNavigation2D);
            PositionPlayerSpawn = GetNode<Position2D>(NodePathPositionPlayerSpawn);
            PositionEnemySpawn = GetNode<Position2D>(NodePathPositionEnemySpawn);
            _gameManager = new GameManager(this);
            _gameManager.CreateMainPlayer(PositionPlayerSpawn.Position);
            //for (int i = 0; i < 5; i++)
                //_gameManager.CreateEnemy(PositionEnemySpawn.Position);
        }
    }

    public class GameManager
    {
        public List<OtherPlayer> Players { get; set; }
        public Player Player { get; set; }
        public List<Enemy> Enemies { get; set; }

        private SceneGame _sceneGame;

        public GameManager(SceneGame sceneGame)
        {
            _sceneGame = sceneGame;
            Enemies = new();
            Players = new();
        }

        public void CreateMainPlayer(Vector2 pos = default(Vector2))
        {
            var player = Prefabs.Player.Instance<Player>();
            player.Position = pos;
            _sceneGame.AddChild(player);
            Player = player;
            Players.Add(player);
        }

        public void CreateOtherPlayer(Vector2 pos = default(Vector2))
        {
            var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
            otherPlayer.Position = pos;
            _sceneGame.AddChild(otherPlayer);
            Players.Add(otherPlayer);
        }

        public void CreateEnemy(Vector2 pos = default(Vector2))
        {
            var enemy = Prefabs.Enemy.Instance<Enemy>();
            enemy.Init(Players, _sceneGame.Navigation2D);
            enemy.Position = pos + Utils.RandomDir() * 10;
            _sceneGame.AddChild(enemy);
            Enemies.Add(enemy);
        }
    }
}
