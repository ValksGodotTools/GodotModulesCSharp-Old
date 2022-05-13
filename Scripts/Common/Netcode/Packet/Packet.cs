namespace GodotModules.Netcode
{
    public abstract class Packet
    {
        public virtual void Write(PacketWriter writer)
        { }

        public virtual void Read(PacketReader reader)
        { }
    }
}