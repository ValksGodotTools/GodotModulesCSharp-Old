namespace GodotModules.Netcode 
{
    public class SPacketLobbyChatMessage : PacketServerPeerId
    {
        public string Message { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
            writer.Write((string)Message);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
            Message = reader.ReadString();
        }

        public override void Handle()
        {
            SceneLobby.Log(Id, Message);
        }
    }
}