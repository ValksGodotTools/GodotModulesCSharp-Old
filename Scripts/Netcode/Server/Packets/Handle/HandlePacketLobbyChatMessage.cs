using GodotModules.Netcode;
using ENet;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Server
{
    public class HandlePacketLobbyChatMessage : HandlePacket
    {
        public override void Handle(Peer peer, PacketReader reader)
        {
            var data = new RPacketLobbyChatMessage(reader);

            Log(data.Message);
        }
    }
}