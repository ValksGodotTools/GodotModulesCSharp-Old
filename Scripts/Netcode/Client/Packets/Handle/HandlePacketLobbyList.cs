using Common.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyList : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new RPacketLobbyList(reader);
            foreach (var player in data.Players)
                UILobby.AddPlayer(player.Key, player.Value);
        }
    }
}