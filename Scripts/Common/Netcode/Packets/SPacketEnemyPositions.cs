using Godot;
using System;

namespace GodotModules.Netcode
{
    public class SPacketEnemyPositions : PacketServer
    {
        private GameOpcode GameOpcode { get; set; }
        public Dictionary<ushort, DataEnemy> Enemies { get; set; }

        public override void Write(PacketWriter writer)
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

        public override void Read(PacketReader reader)
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

#if CLIENT
        public override async Task Handle()
        {
            //var sceneGameScript = SceneManager.GetActiveSceneScript<Game.SceneGame>();

            //sceneGameScript.EnemyTransformQueue.Add(Enemies);

            /*foreach (var pair in Enemies)
            {
                var enemy = sceneGameScript.Enemies[pair.Key];
                if (enemy.Position.DistanceTo(pair.Value.Position) > 100) // TODO: Lerp
                    sceneGameScript.Enemies[pair.Key].Position = pair.Value.Position;
            }*/

            await Task.FromResult(1);
        }
#endif
    }
}