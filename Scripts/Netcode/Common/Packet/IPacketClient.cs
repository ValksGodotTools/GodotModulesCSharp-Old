namespace GodotModules.Netcode
{
    public interface IPacketClient : IPacket
    {
        /// <summary>
        /// The packet handled server-side
        /// 
        /// IMPORTANT:
        /// GameServer.Log() should be the only thing used to log values (System.Console.WriteLine() and Godot.GD.Print() will NOT work)
        /// </summary>
        /// <param name="peer">The client peer</param>
        void Handle(ENet.Peer peer);
    }
}