using Godot;

namespace GodotModules 
{
    public class SceneGame : AScene
    {
        [Export] protected readonly NodePath NodePathPositionPlayerSpawn;
        [Export] protected readonly NodePath NodePathPositionEnemySpawn;
        [Export] protected readonly NodePath NodePathNavigation2D;
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
}
