namespace GodotModules.Netcode
{
    public interface IPacketClient : IPacket
    {
        /// <summary>
        /// The packet handled server-side
        /// </summary>
        /// <param name="peer">The client peer</param>
        void Handle(ENet.Peer peer);
    }
}