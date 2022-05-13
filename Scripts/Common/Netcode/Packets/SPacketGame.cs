using Godot;
using System;

namespace GodotModules.Netcode
{
    public class SPacketGame : PacketServer
    {
        private GameOpcode GameOpcode { get; set; }
        public Dictionary<ushort, DataEnemy> Enemies { get; set; }

        public SPacketGame() { } // need for reflection

        public SPacketGame(GameOpcode opcode)
        {
            GameOpcode = opcode;
        }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)GameOpcode);

            if (GameOpcode == GameOpcode.EnemiesSpawned)
            {
                writer.Write((ushort)Enemies.Count);
                foreach (var pair in Enemies)
                {
                    writer.Write((ushort)pair.Key); // id
                    var pos = pair.Value.Position;
                    writer.Write((float)Math.Round(pos.x, 1));
                    writer.Write((float)Math.Round(pos.y, 1));
                }
            }
        }

        public override void Read(PacketReader reader)
        {
            GameOpcode = (GameOpcode)reader.ReadByte();

            if (GameOpcode == GameOpcode.EnemiesSpawned)
            {
                Enemies = new Dictionary<ushort, DataEnemy>();
                var count = reader.ReadUShort();
                for (int i = 0; i < count; i++)
                {
                    var id = reader.ReadUShort();
                    var pos = Vector2.Zero;
                    pos.x = reader.ReadFloat();
                    pos.y = reader.ReadFloat();

                    Enemies.Add(id, new DataEnemy
                    {
                        Position = pos
                    });
                }
            }
        }

#if CLIENT
        public override async Task Handle()
        {
            /*var sceneGameScript = SceneManager.GetActiveSceneScript<Game.SceneGame>();

            foreach (var pair in Enemies)
            {
                var enemy = Prefabs.Enemy.Instance<GodotModules.Netcode.Server.Enemy>();
                enemy.Position = pair.Value.Position;
                enemy.SetPlayers(sceneGameScript.Players);
                sceneGameScript.AddChild(enemy);
                sceneGameScript.Enemies[pair.Key] = enemy;
            }*/

            await Task.FromResult(1);
        }
#endif
    }
}