namespace GodotModules.Netcode
{
    public abstract class APacketServer : APacket
    {
        /// <summary>
        /// The packet handled client-side
        /// </summary>
        public abstract void Handle();
    }
}