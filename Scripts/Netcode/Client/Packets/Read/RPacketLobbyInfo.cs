using Common.Netcode;
using System.Collections.Generic;

namespace GodotModules.Netcode.Client 
{
    public class RPacketLobbyInfo
    {
        public uint Id { get; set; }
        public Dictionary<uint, string> Players { get; set; }

        public RPacketLobbyInfo(PacketReader reader) 
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