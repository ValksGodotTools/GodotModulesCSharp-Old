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
            var time = DateTime.Now - CommandDebug.timeSent;

            NetworkManager.GameServer.Log($"Received pong, {time.Milliseconds}ms");
        }
    }
}