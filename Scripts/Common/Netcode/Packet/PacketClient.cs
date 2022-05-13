namespace GodotModules.Netcode
{
    public abstract class PacketClient : Packet
    {
        /// <summary>
        /// The packet handled server-side
        /// </summary>
        /// <param name="peer">The client peer</param>
        public abstract void Handle(ENet.Peer peer);
    }
}