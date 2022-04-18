using Common.Netcode;
using ENet;

namespace GodotModules.Netcode.Client
{
    public abstract class HandlePacket : GameClient
    {
        /// <summary>
        /// This is in the Godot thread, anything from the Godot thread can be used here
        /// </summary>
        /// <param name="reader"></param>
        public abstract void Handle(PacketReader reader);
    }
}