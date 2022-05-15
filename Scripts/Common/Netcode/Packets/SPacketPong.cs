using GodotModules.Netcode.Client;

namespace GodotModules.Netcode
{
    public class SPacketPong : APacketServer
    {
#if CLIENT
        public override async Task Handle(GameClient client)
        {
            //NetworkManager.WasPingReceived = true;
            //NetworkManager.PingMs = (DateTime.Now - NetworkManager.PingSent).Milliseconds;
            await Task.FromResult(1);
        }
#endif
    }
}