using ENet;
using System;
using System.IO;

namespace GodotModules.Netcode
{
    public class PacketReader : IDisposable
    {
        private MemoryStream Stream { get; set; }
        private BinaryReader Reader { get; set; }
        private readonly byte[] ReadBuffer = new byte[GamePacket.MaxSize];

        public PacketReader(Packet packet)
        {
            Stream = new(ReadBuffer);
            Reader = new(Stream);
            packet.CopyTo(ReadBuffer);
            packet.Dispose();
        }

        public byte ReadByte() => Reader.ReadByte();

        public sbyte ReadSByte() => Reader.ReadSByte();

        public char ReadChar() => Reader.ReadChar();

        public string ReadString() => Reader.ReadString();

        public bool ReadBool() => Reader.ReadBoolean();

        public short ReadShort() => Reader.ReadInt16();

        public ushort ReadUShort() => Reader.ReadUInt16();

        public int ReadInt() => Reader.ReadInt32();

        public uint ReadUInt() => Reader.ReadUInt32();

        public float ReadFloat() => Reader.ReadSingle();

        public double ReadDouble() => Reader.ReadDouble();

        public long ReadLong() => Reader.ReadInt64();

        public ulong ReadULong() => Reader.ReadUInt64();

        public byte[] ReadBytes(int count) => Reader.ReadBytes(count);

        public void Dispose()
        {
            Stream.Dispose();
            Reader.Dispose();
        }
    }
}