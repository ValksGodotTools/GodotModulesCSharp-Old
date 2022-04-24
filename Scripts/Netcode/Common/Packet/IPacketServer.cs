namespace GodotModules.Netcode
{
    public interface IPacketServer : IPacket
    {
        /// <summary>
        /// The packet handled client-side
        /// </summary>
        void Handle();
    }
}