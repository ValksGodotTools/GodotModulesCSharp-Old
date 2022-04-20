using GodotModules.Netcode;

namespace GodotModules.Netcode.Server
{
    public class RPacketLobbyChatMessage
    {
        public string Message { get; set; }

        public RPacketLobbyChatMessage(PacketReader reader)
        {
            Message = reader.ReadString();
        }
    }
}