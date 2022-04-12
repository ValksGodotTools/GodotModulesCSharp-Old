namespace Common.Netcode
{
    public interface IWritable 
    {
        void Write(PacketWriter writer);
    }
}