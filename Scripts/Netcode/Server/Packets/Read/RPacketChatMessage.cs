using Common.Netcode;

namespace Valk.Modules.Netcode.Server
{
    public class RPacketChatMessage : IReadable
    {
        public uint ChannelId { get; set; }
        public string Message { get; set; }

        public void Read(PacketReader reader)
        {
            ChannelId = reader.ReadUInt32();
            Message = reader.ReadString();
        }
    }
}