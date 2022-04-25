using System;
using ENet;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyChatMessage : IPacketClient
    {
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Message);
        }

        public void Read(PacketReader reader)
        {
            Message = reader.ReadString();
        }

        public void Handle(Peer peer)
        {
            GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyChatMessage, new SPacketLobbyChatMessage {
                Id = peer.ID,
                Message = Message
            });
        }
    }
}