namespace GodotModules.Netcode
{
    public interface IPacketClient : IPacket
    {
        void Handle(ENet.Peer peer);
    }
}