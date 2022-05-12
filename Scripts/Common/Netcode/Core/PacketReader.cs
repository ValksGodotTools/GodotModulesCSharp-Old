using ENet;
using System;
using System.IO;

namespace GodotModules.Netcode
{
    public class PacketReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;
        private readonly byte[] ReadBuffer = new byte[GamePacket.MaxSize];

        public PacketReader(Packet packet)
        {
            _stream = new(ReadBuffer);
            _reader = new(_stream);
            packet.CopyTo(ReadBuffer);
            packet.Dispose();
        }

        public byte ReadByte() => _reader.ReadByte();
        public sbyte ReadSByte() => _reader.ReadSByte();
        public char ReadChar() => _reader.ReadChar();
        public string ReadString() => _reader.ReadString();
        public bool ReadBool() => _reader.ReadBoolean();
        public short ReadShort() => _reader.ReadInt16();
        public ushort ReadUShort() => _reader.ReadUInt16();
        public int ReadInt() => _reader.ReadInt32();
        public uint ReadUInt() => _reader.ReadUInt32();
        public float ReadFloat() => _reader.ReadSingle();
        public double ReadDouble() => _reader.ReadDouble();
        public long ReadLong() => _reader.ReadInt64();
        public ulong ReadULong() => _reader.ReadUInt64();
        public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }
}