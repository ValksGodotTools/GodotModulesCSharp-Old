using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyReady : IPacketServer
    {
        public uint Id { get; set; }
        public bool Ready { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(Ready);
        }

        public void Read(PacketReader reader)
        {
            Ready = reader.ReadBool();
        }

        public void Handle()
        {
            
        }
    }
}