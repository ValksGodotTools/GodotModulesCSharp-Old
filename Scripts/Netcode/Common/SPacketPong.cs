using Game;
using Godot;
using GodotModules.Netcode.Server;
using GodotModules.Netcode.Client;
using System;

namespace GodotModules.Netcode
{
    public class SPacketPong : APacketServer
    {
        public override void Handle(ENetClient client)
        {
            client.WasPingReceived = true;
            client.PingMs = (DateTime.Now - client.PingSent).Milliseconds;
        }
    }
}