using System;
using System.IO;

namespace GodotModules.Netcode
{
    public class PacketWriter : IDisposable
    {
        public MemoryStream Stream { get; set; }
        public BinaryWriter Writer { get; set; }

        public PacketWriter()
        {
            Stream = new();
            Writer = new(Stream);
        }

        public void Write(byte v) => Writer.Write(v);

        public void Write(sbyte v) => Writer.Write(v);

        public void Write(char v) => Writer.Write(v);

        public void Write(string v) => Writer.Write(v);

        public void Write(bool v) => Writer.Write(v);

        public void Write(short v) => Writer.Write(v);

        public void Write(ushort v) => Writer.Write(v);

        public void Write(int v) => Writer.Write(v);

        public void Write(uint v) => Writer.Write(v);

        public void Write(float v) => Writer.Write(v);

        public void Write(double v) => Writer.Write(v);

        public void Write(long v) => Writer.Write(v);

        public void Write(ulong v) => Writer.Write(v);

        public void Write(byte[] v) => Writer.Write(v);

        public void Dispose()
        {
            Stream.Dispose();
            Writer.Dispose();
        }
    }
}