using System.IO;

namespace GodotModules.Netcode 
{
    public abstract class APacketServerPeerId : APacketServer
    {
        public uint Id { get; set; }

        public override void Write(PacketWriter writer) => writer.Write((ushort)Id);
        public override void Read(PacketReader reader) => Id = reader.ReadUShort();
    }
}