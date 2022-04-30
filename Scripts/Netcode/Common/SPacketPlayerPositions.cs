using Game;
using Godot;
using System;

namespace GodotModules.Netcode
{
    public class SPacketPlayerPositions : APacketServer
    {
        public Dictionary<uint, Vector2> PlayerPositions { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((ushort)PlayerPositions.Count);
            PlayerPositions.ForEach(pair =>
            {
                writer.Write(pair.Key); // id
                writer.Write((float)Math.Round(pair.Value.x, 1));
                writer.Write((float)Math.Round(pair.Value.y, 1));
            });
        }

        public override void Read(PacketReader reader)
        {
            PlayerPositions = new Dictionary<uint, Vector2>();
            var count = reader.ReadUShort();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadUInt();
                var x = reader.ReadFloat();
                var y = reader.ReadFloat();

                PlayerPositions[id] = new Vector2(x, y);
            }
        }

        public override void Handle()
        {
            SceneGame.UpdatePlayerPositions(PlayerPositions);
        }
    }
}