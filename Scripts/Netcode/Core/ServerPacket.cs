using LiteNetLib;
using LiteNetLib.Utils;

namespace GodotModules.Netcode
{
    public class ServerPacket : GamePacket
    {
        public NetPeer[] Peers { get; private set; }

        public ServerPacket(byte opcode, DeliveryMethod deliveryMethod, APacket writable = null, params NetPeer[] peers)
        {
            NetDataWriter = new NetDataWriter();
            NetDataWriter.Put(opcode);
            writable?.Write(NetDataWriter);

            Opcode = opcode;
            DeliveryMethod = deliveryMethod;
            Peers = peers;
        }
    }
}