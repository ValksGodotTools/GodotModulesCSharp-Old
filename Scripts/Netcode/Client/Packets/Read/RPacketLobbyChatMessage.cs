using GodotModules.Netcode;

namespace GodotModules.Netcode.Client 
{
    public class RPacketLobbyChatMessage
    {
        public uint Id { get; set; }
        public string Message { get; set; }

        public RPacketLobbyChatMessage(PacketReader reader)
        {
            Id = reader.ReadUInt16();
            Message = reader.ReadString();
        }
    }
}