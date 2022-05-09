using System;
using System.Threading.Tasks;

namespace GodotModules.Netcode
{
    public class SPacketPong : APacketServer
    {
#if CLIENT
        public override async Task Handle()
        {
            NetworkManager.WasPingReceived = true;
            NetworkManager.PingMs = (DateTime.Now - NetworkManager.PingSent).Milliseconds;
            await Task.FromResult(1);
        }
#endif
    }
}