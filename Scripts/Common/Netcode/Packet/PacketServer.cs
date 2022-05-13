using System.Threading.Tasks;

namespace GodotModules.Netcode
{
    public abstract class PacketServer : Packet
    {
        /// <summary>
        /// The packet handled client-side
        /// </summary>
        public virtual Task Handle() => Task.FromResult(1);
    }
}