using LiteNetLib;
using LiteNetLib.Utils;

namespace GodotModules.Netcode
{
    public class ClientPacket : GamePacket
    {
        public ClientPacket(byte opcode, DeliveryMethod deliveryMethod, APacket writable = null)
        {
            NetDataWriter = new NetDataWriter();
            NetDataWriter.Put(opcode);
            writable?.Write(NetDataWriter);

            Opcode = opcode;
            DeliveryMethod = deliveryMethod;
        }
    }
}