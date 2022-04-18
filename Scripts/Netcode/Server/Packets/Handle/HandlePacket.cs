using Common.Netcode;
using ENet;

namespace GodotModules.Netcode.Server
{
    public abstract class HandlePacket : GameServer
    {
        /// <summary>
        /// This is in the ENet thread, anything from the ENet thread can be used here
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="reader"></param>
        public abstract void Handle(Peer peer, PacketReader reader);
    }
}