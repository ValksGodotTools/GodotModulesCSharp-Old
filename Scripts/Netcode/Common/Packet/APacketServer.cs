using GodotModules.Netcode.Client;
using System.Threading.Tasks;

namespace GodotModules.Netcode
{
    public abstract class APacketServer : APacket
    {
        /// <summary>
        /// The packet handled client-side
        /// </summary>
        public abstract Task Handle(ENetClient client);
    }
}