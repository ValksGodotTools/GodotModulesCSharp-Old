namespace GodotModules.Netcode
{
    public class CPacketPing : APacketClient
    {
        public override void Handle(ENet.Peer peer)
        {
            NetworkManager.GameServer.Send(ServerPacketOpcode.Pong, peer);
        }
    }
}