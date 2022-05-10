using Godot;

namespace GodotModules.Netcode.Server
{
    public class ServerSimulation : Node
    {
        private ConcurrentQueue<ThreadCmd<SimulationOpcode>> ServerSimulationQueue = new ConcurrentQueue<ThreadCmd<SimulationOpcode>>();

        private Dictionary<ushort, Enemy> Enemies = new Dictionary<ushort, Enemy>();
        private Dictionary<byte, Game.OtherPlayer> Players;
        private GTimer Timer { get; set; }
        private Dictionary<byte, PrevCurQueue<Vector2>> PlayerPositions = new Dictionary<byte, PrevCurQueue<Vector2>>();

        public override void _Ready()
        {
            Players = new Dictionary<byte, Game.OtherPlayer>();
            Timer = new GTimer(ServerIntervals.PlayerTransforms, true, false);
            Timer.Connect(this, nameof(EmitSimulationData));
        }

        public override void _PhysicsProcess(float delta)
        {
            foreach (var pair in PlayerPositions)
            {
                var prev = pair.Value.Previous;
                var cur = pair.Value.Current;

                pair.Value.UpdateProgress(delta);

                Players[pair.Key].Position = Utils.Lerp(prev, cur, pair.Value.Progress);
            }
        }

        public void Enqueue(ThreadCmd<SimulationOpcode> cmd) => ServerSimulationQueue.Enqueue(cmd);

        public void Dequeue()
        {
            if (ServerSimulationQueue.TryDequeue(out ThreadCmd<SimulationOpcode> cmd))
            {
                var opcode = cmd.Opcode;

                if (opcode == SimulationOpcode.StartSimulation)
                {
                    Timer.Start();
                }

                if (opcode == SimulationOpcode.CreatePlayer)
                {
                    var id = (byte)cmd.Data;
                    CreatePlayer(id);
                }

                if (opcode == SimulationOpcode.CreateEnemy)
                {
                    var enemy = (SimulationEnemy)cmd.Data;
                    CreateEnemy(enemy);
                }

                if (opcode == SimulationOpcode.PlayerPosition)
                {
                    var player = (SimulationPlayerPosition)cmd.Data;
                    if (!PlayerPositions.ContainsKey(player.Id)) 
                    {
                        PlayerPositions[player.Id] = new PrevCurQueue<Vector2>(ClientIntervals.PlayerPosition);
                        PlayerPositions[player.Id].Add(player.Position);
                    }
                    else 
                    {
                        PlayerPositions[player.Id].Add(player.Position);
                    }
                    
                    //Players[player.Id].Position = player.Position;
                }
            }
        }

        private void CreatePlayer(byte id)
        {
            var otherPlayer = Prefabs.OtherPlayer.Instance<Game.OtherPlayer>();
            otherPlayer.AddToGroup("Player");
            otherPlayer.Position = Vector2.Zero;
            Players.Add(id, otherPlayer);
            AddChild(otherPlayer);
        }

        private void CreateEnemy(SimulationEnemy simEnemy)
        {
            var enemy = Prefabs.Enemy.Instance<Enemy>();
            enemy.AddToGroup("Enemy");
            enemy.SetPlayers(Players);
            enemy.Position = simEnemy.SpawnForce;
            Enemies.Add(simEnemy.Id, enemy);
            AddChild(enemy);
        }

        private void EmitSimulationData()
        {
            var enemyData = Enemies.ToDictionary(x => x.Key, x => new DataEnemy {
                Position = x.Value.Position,
                Rotation = x.Value.Rotation
            });

            NetworkManager.GameServer.ENetCmds.Enqueue(new ThreadCmd<ENetOpcode>(ENetOpcode.EnemyTransforms, enemyData));
        }

        public void Cleanup()
        {
            Timer.Stop();
            while (ServerSimulationQueue.TryDequeue(out _));
            Enemies.Clear();
            Players.Clear();
        }
    }
}
