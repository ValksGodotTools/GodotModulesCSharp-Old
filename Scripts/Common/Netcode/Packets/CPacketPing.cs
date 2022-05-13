namespace GodotModules.Netcode
{
    public class CPacketPing : PacketClient
    {
        public override void Handle(ENet.Peer peer)
        {
            GM.Net.Server.Send(ServerPacketOpcode.Pong, peer);
        }
    }
}