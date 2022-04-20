using GodotModules.Netcode;

namespace GodotModules.Netcode.Client 
{
    public class RPacketLobbyLeave
    {
        public uint Id { get; set; }

        public RPacketLobbyLeave(PacketReader reader)
        {
            Id = reader.ReadUInt16();
        }
    }
}