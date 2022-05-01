using Godot;
using GodotModules.Netcode.Server;
using System;

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