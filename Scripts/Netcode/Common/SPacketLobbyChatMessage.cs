namespace GodotModules.Netcode 
{
    public class SPacketLobbyChatMessage : APacketServerPeerId
    {
        public LobbyOpcode LobbyOpcode { get; set; }

        // Chat Message
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
            if (!SceneManager.InLobby())
                return;
            
            SceneLobby.Log(Id, Message);
        }
    }
}