
using LiteNetLib;
using LiteNetLib.Utils;

namespace GodotModules.Netcode
{
    public class GamePacket
    {
        public const int MaxSize = 8192;
        public DeliveryMethod DeliveryMethod = DeliveryMethod.ReliableOrdered; // Lets make packets reliable by default
        public long Length { get; protected set; }
        public byte Opcode { get; protected set; }
        public NetDataWriter NetDataWriter { get; protected set; }
    }
}