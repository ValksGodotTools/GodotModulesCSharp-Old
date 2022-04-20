using GodotModules.Netcode;

namespace GodotModules.Netcode.Client
{
    public class WPacketLobbyChatMessage : IWritable
    {
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Message);
        }
    }
}