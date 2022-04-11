namespace Common.Netcode
{
    public interface IReadable 
    {
        void Read(PacketReader reader);
    }
}