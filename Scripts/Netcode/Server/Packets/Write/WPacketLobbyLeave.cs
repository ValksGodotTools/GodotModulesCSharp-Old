using GodotModules.Netcode;

namespace GodotModules.Netcode.Server
{
    public class WPacketLobbyLeave : IWritable 
    {
        public uint Id { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write((ushort)Id);
        }
    }
}