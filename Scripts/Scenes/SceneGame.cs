using Godot;

namespace GodotModules 
{
    public class SceneGame : AScene
    {
        public static SceneGame Instance { get; private set; }

        [Export] protected readonly NodePath NodePathPositionPlayerSpawn;
        [Export] protected readonly NodePath NodePathPositionEnemySpawn;
        [Export] protected readonly NodePath NodePathNavigation2D;

        public Navigation2D Navigation2D { get; set; }
        public Position2D PositionPlayerSpawn { get; set; }
        public Position2D PositionEnemySpawn { get; set; }

        public List<OtherPlayer> Players { get; set; }
        public Player Player { get; set; }
        public List<Enemy> Enemies { get; set; }

        public override void PreInitManagers(Managers managers)
        {
            
        }

        public override void _Ready()
        {
            Instance = this;
            
            Navigation2D = GetNode<Navigation2D>(NodePathNavigation2D);
            PositionPlayerSpawn = GetNode<Position2D>(NodePathPositionPlayerSpawn);
            PositionEnemySpawn = GetNode<Position2D>(NodePathPositionEnemySpawn);

            Enemies = new();
            Players = new();

            CreateMainPlayer(PositionPlayerSpawn.Position);
            //for (int i = 0; i < 5; i++)
                //_gameManager.CreateEnemy(PositionEnemySpawn.Position);
        }

        public void CreateMainPlayer(Vector2 pos = default(Vector2))
        {
            var player = Prefabs.Player.Instance<Player>();
            player.Position = pos;
            AddChild(player);
            Player = player;
            Players.Add(player);
        }

        public void CreateOtherPlayer(Vector2 pos = default(Vector2))
        {
            var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
            otherPlayer.Position = pos;
            AddChild(otherPlayer);
            Players.Add(otherPlayer);
        }

        public void CreateEnemy(Vector2 pos = default(Vector2))
        {
            var enemy = Prefabs.Enemy.Instance<Enemy>();
            enemy.Init(Players, Navigation2D);
            enemy.Position = pos + Utils.RandomDir() * 10;
            AddChild(enemy);
            Enemies.Add(enemy);
        }
    }
}
