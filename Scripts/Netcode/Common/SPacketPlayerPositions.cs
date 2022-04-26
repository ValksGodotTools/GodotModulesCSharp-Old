using GodotModules.Netcode.Client;
using System;
using System.Collections.Generic;
using Game;
using Godot;

namespace GodotModules.Netcode 
{
    public class SPacketPlayerPositions : APacketServer
    {
        public Dictionary<uint, Vector2> PlayerPositions { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write((ushort)PlayerPositions.Count);
            foreach (var pair in PlayerPositions)
            {
                var id = pair.Key;
                var pos = pair.Value;

                var x = (float)Math.Round(pos.x, 1);
                var y = (float)Math.Round(pos.y, 1);

                writer.Write(pair.Key);
                writer.Write(x);
                writer.Write(y);
            }
        }

        public override void Read(PacketReader reader)
        {
            PlayerPositions = new Dictionary<uint, Vector2>();
            var count = reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadUInt32();
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