using System.IO;
using ENet;

namespace Common.Netcode
{
    public class PacketReader : BinaryReader 
    {
        private static readonly byte[] ReadBuffer = new byte[GamePacket.MaxSize];

        public PacketReader(Packet packet) : base(new MemoryStream(ReadBuffer)) 
        {
            BaseStream.Position = 0;
            packet.CopyTo(ReadBuffer);
            packet.Dispose();
        }

        public bool ReadBool() => base.ReadBoolean();
        public sbyte ReadInt8() => base.ReadSByte();
        public byte ReadUInt8() => base.ReadByte();
        public float ReadFloat() => base.ReadSingle();
    }
}