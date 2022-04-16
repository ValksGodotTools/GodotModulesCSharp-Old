using Common.Netcode;
using System.Collections.Generic;

namespace GodotModules.Netcode.Client 
{
    public class RPacketLobbyList 
    {
        public Dictionary<uint, string> Players { get; set; }

        public RPacketLobbyList(PacketReader reader) 
        {
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