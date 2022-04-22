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
            var data = new CPacketLobbyChatMessage();
            data.Read(reader);

            Send(ServerPacketOpcode.LobbyChatMessage, new SPacketLobbyChatMessage {
                Id = peer.ID,
                Message = data.Message
            }, GetAllPlayerPeers());
        }
    }
}