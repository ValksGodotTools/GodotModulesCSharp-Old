using Common.Netcode;
using ENet;
using Godot;

namespace Valk.Modules.Netcode.Client
{
    public abstract class HandlePacket
    {
        public abstract ServerPacketOpcode Opcode { get; set; }

        public abstract void Handle(PacketReader packetReader);
    }
}