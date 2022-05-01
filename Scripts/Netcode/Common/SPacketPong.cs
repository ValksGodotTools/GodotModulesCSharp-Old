using Game;
using Godot;
using GodotModules.Netcode.Server;
using System;

namespace GodotModules.Netcode
{
    public class SPacketPong : APacketServer
    {
        public override void Handle()
        {
            PingServers.PingMs = (DateTime.Now - PingServers.PingSent).Milliseconds;
            PingServers.PingReceived = true;
        }
    }
}