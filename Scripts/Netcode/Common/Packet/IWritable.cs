namespace GodotModules.Netcode
{
    public interface IWritable
    {
        void Write(PacketWriter writer);
    }
}