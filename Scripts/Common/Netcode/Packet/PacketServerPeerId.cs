namespace GodotModules.Netcode
{
    public abstract class PacketServerPeerId : PacketServer
    {
        public byte Id { get; set; }

        public override void Write(PacketWriter writer) => writer.Write(Id);

        public override void Read(PacketReader reader) => Id = reader.ReadByte();
    }
}