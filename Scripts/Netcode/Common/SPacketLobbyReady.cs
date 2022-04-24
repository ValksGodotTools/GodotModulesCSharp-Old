using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyReady : PacketServerPeerId
    {
        public bool Ready { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.Write(Ready);
        }

        public override void Read(PacketReader reader)
        {
            Ready = reader.ReadBool();
        }

        public override void Handle()
        {
            
        }
    }
}