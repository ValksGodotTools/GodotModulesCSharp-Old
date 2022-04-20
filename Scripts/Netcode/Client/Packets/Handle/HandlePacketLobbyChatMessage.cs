using GodotModules;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyChatMessage : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new RPacketLobbyChatMessage(reader);

            SceneLobby.Log(data.Message);
        }
    }
}