using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class CPacketLobbyChatMessage : APacketClient
    {
        public string Message { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Message);
        }

        public override void Read(PacketReader reader)
        {
            Message = reader.ReadString();
        }

        public override void Handle(ENet.Peer peer)
        {
            GameServer.SendToAllPlayers(ServerPacketOpcode.LobbyChatMessage, new SPacketLobbyChatMessage {
                Id = peer.ID,
                Message = Message
            });
        }
    }
}