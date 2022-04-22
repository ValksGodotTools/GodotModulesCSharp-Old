namespace GodotModules.Netcode
{
    public interface IPacketServer : IPacket
    {
        void Handle();
    }
}