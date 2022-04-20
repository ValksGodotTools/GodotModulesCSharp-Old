using GodotModules.Netcode;

namespace GodotModules.Netcode.Server
{
    public class WPacketLobbyChatMessage : IWritable 
    {
        public uint Id { get; set; }
        public string Message { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
            writer.Write((string)Message);
        }
    }
}