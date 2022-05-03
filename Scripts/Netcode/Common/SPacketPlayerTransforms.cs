using Game;
using Godot;
using GodotModules.Netcode.Client;
using System;

namespace GodotModules.Netcode
{
    public class SPacketPlayerTransforms : APacketServer
    {
        public Dictionary<byte, Vector2> PlayerPositions { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((byte)PlayerPositions.Count);
            PlayerPositions.ForEach(pair =>
            {
                writer.Write((byte)pair.Key); // id
                writer.Write((float)Math.Round(pair.Value.x, 1));
                writer.Write((float)Math.Round(pair.Value.y, 1));
            });
        }

        public override void Read(PacketReader reader)
        {
            PlayerPositions = new Dictionary<byte, Vector2>();
            var count = reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadByte();
                var x = reader.ReadFloat();
                var y = reader.ReadFloat();

                PlayerPositions[id] = new Vector2(x, y);
            }
        }

        public override void Handle(ENetClient client)
        {
            SceneGame.UpdatePlayerPositions(PlayerPositions);
        }
    }
}