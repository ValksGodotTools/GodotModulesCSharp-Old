using Godot;
using System;

namespace GodotModules.Netcode
{
    public class SPacketPlayerTransforms : APacketServer
    {
        public Dictionary<byte, DataTransform> PlayerTransforms { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)PlayerTransforms.Count);
            PlayerTransforms.ForEach(pair =>
            {
                var transform = pair.Value;

                writer.Write((byte)pair.Key); // id
                writer.Write((float)Math.Round(transform.Position.x, 1));
                writer.Write((float)Math.Round(transform.Position.y, 1));
                writer.Write((float)Math.Round(transform.Rotation, 1));
            });
        }

        public override void Read(PacketReader reader)
        {
            PlayerTransforms = new Dictionary<byte, DataTransform>();
            var count = reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadByte();
                var x = reader.ReadFloat();
                var y = reader.ReadFloat();
                var rot = reader.ReadFloat();

                PlayerTransforms[id] = new DataTransform {
                    Position = new Vector2(x, y),
                    Rotation = rot
                };
            }
        }

#if CLIENT
        public override async Task Handle()
        {
            /*if (SceneManager.InGame())
                SceneManager.GetActiveSceneScript<Game.SceneGame>().UpdatePlayerPositions(PlayerTransforms);*/
            await Task.FromResult(1);
        }
#endif
    }
}