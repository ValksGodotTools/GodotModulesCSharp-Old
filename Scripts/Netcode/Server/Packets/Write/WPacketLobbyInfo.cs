using Common.Netcode;
using System.Collections.Generic;

namespace GodotModules.Netcode.Server 
{
    public class WPacketLobbyInfo : IWritable 
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
    }
}