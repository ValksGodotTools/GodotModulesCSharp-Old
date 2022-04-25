namespace GodotModules.Netcode
{
    public abstract class APacketClient : APacket
    {
        /// <summary>
        /// The packet handled server-side
        /// 
        /// IMPORTANT:
        /// GameServer.Log() should be the only thing used to log values (System.Console.WriteLine() and Godot.GD.Print() will NOT work)
        /// </summary>
        /// <param name="peer">The client peer</param>
        public abstract void Handle(ENet.Peer peer);
    }
}