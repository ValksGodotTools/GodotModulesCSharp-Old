using GodotModules;

namespace GodotModules.Netcode.Client
{
    public class HandlePacketLobbyChatMessage : HandlePacket
    {
        public override void Handle(PacketReader reader)
        {
            var data = new SPacketLobbyChatMessage();
            data.Read(reader);

            SceneLobby.Log(data.Id, data.Message);
        }
    }
}