namespace GodotModules.Netcode 
{
    public abstract class PacketServerPeerId : IPacketServer
    {
        public uint Id { get; set; }

        public virtual void Write(PacketWriter writer) => writer.Write((ushort)Id);
        public virtual void Read(PacketReader reader) => reader.ReadUInt16();
        public abstract void Handle();
    }
}