using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyReady : APacketServerPeerId
    {
        public bool Ready { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
            writer.Write(Ready);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
            Ready = reader.ReadBool();
        }

        public override void Handle()
        {
            if (!SceneManager.InLobby())
                return;
                
            SceneLobby.SetReady(Id, Ready);
        }
    }
}