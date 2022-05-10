using Godot;
using System;

namespace GodotModules.Netcode.Server
{
    public class ServerSimulation : Node
    {
        private static ConcurrentQueue<ThreadCmd<SimulationOpcode>> ServerSimulationQueue = new ConcurrentQueue<ThreadCmd<SimulationOpcode>>();

        public static Dictionary<ushort, Enemy> Enemies = new Dictionary<ushort, Enemy>();
        public static Dictionary<byte, Game.OtherPlayer> Players = new Dictionary<byte, Game.OtherPlayer>();

        private static ServerSimulation Instance { get; set; }
        private static GTimer Timer { get; set; }
        private static Dictionary<byte, PrevCurQueue<Vector2>> PlayerPositions = new Dictionary<byte, PrevCurQueue<Vector2>>();

        public override void _Ready()
        {
            Instance = this;
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

        public static void Enqueue(ThreadCmd<SimulationOpcode> cmd) => ServerSimulationQueue.Enqueue(cmd);

        public static void Dequeue()
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

        private static void CreatePlayer(byte id)
        {
            var otherPlayer = Prefabs.OtherPlayer.Instance<Game.OtherPlayer>();
            otherPlayer.AddToGroup("Player");
            otherPlayer.Position = Vector2.Zero;
            Players.Add(id, otherPlayer);
            Instance.AddChild(otherPlayer);
        }

        private static void CreateEnemy(SimulationEnemy simEnemy)
        {
            var enemy = Prefabs.Enemy.Instance<Enemy>();
            enemy.AddToGroup("Enemy");
            enemy.Position = simEnemy.SpawnForce;
            Enemies.Add(simEnemy.Id, enemy);
            Instance.AddChild(enemy);
        }

        private void EmitSimulationData()
        {
            var enemyData = Enemies.ToDictionary(x => x.Key, x => new DataEnemy {
                Position = x.Value.Position,
                Rotation = x.Value.Rotation
            });

            NetworkManager.GameServer.ENetCmds.Enqueue(new ThreadCmd<ENetOpcode>(ENetOpcode.EnemyTransforms, enemyData));
        }

        public static void Cleanup()
        {
            Timer.Stop();
            while (ServerSimulationQueue.TryDequeue(out _));
            Enemies.Clear();
            Players.Clear();
        }
    }
}
