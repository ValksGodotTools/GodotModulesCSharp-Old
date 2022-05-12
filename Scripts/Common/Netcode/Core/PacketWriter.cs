using System;
using System.IO;

namespace GodotModules.Netcode
{
    public class PacketWriter : IDisposable
    {
        public MemoryStream Stream { get; }
        private readonly BinaryWriter _writer;

        public PacketWriter()
        {
            Stream = new();
            _writer = new(Stream);
        }

        public void Write(byte v) => _writer.Write(v);
        public void Write(sbyte v) => _writer.Write(v);
        public void Write(char v) => _writer.Write(v);
        public void Write(string v) => _writer.Write(v);
        public void Write(bool v) => _writer.Write(v);
        public void Write(short v) => _writer.Write(v);
        public void Write(ushort v) => _writer.Write(v);
        public void Write(int v) => _writer.Write(v);
        public void Write(uint v) => _writer.Write(v);
        public void Write(float v) => _writer.Write(v);
        public void Write(double v) => _writer.Write(v);
        public void Write(long v) => _writer.Write(v);
        public void Write(ulong v) => _writer.Write(v);
        public void Write(byte[] v) => _writer.Write(v);

        public void Dispose()
        {
            Stream.Dispose();
            _writer.Dispose();
        }
    }
}