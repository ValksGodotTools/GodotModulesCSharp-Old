using GodotModules.Netcode;
using ENet;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyChatMessage : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new RPacketLobbyChatMessage(reader);
        }
    }
}