using Common.Netcode;
using System.Collections.Generic;

namespace GodotModules.Netcode.Server 
{
    public class WPacketLobbyList : IWritable 
    {
        public Dictionary<uint, string> Players { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Players.Count);
            foreach (var player in Players)
            {
                writer.Write((ushort)player.Key);
                writer.Write((string)player.Value);
            }
        }
    }
}