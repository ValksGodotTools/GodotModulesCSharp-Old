using Game;
using Godot;
using GodotModules.Netcode.Server;
using GodotModules.Netcode.Client;
using System;
using System.Threading.Tasks;

namespace GodotModules.Netcode
{
    public class SPacketPong : APacketServer
    {
#if CLIENT
        public override async Task Handle(ENetClient client)
        {
            client.WasPingReceived = true;
            client.PingMs = (DateTime.Now - client.PingSent).Milliseconds;
            await Task.FromResult(1);
        }
#endif
    }
}