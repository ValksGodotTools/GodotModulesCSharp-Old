using System.Collections.Generic;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyInfo : IPacket
    {
        public uint Id { get; set; }
        public Dictionary<uint, string> Players { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((ushort)Players.Count);
            foreach (var player in Players)
            {
                writer.Write((ushort)player.Key);
                writer.Write((string)player.Value);
            }
        }

        public void Read(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            var count = reader.ReadUInt16();
            Players = new Dictionary<uint, string>();
            for (int i = 0; i < count; i++)
            {
                var id = reader.ReadUInt16();
                var name = reader.ReadString();

                Players.Add(id, name);
            }
        }
    }
}