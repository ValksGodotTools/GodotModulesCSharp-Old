using GodotModules.Netcode.Client;

namespace GodotModules.Netcode 
{
    public class SPacketLobbyCountdownChange : APacketServerPeerId
    {
        public bool CountdownRunning { get; set; }

        public override void Write(PacketWriter writer)
        {
            base.Write(writer);
            writer.Write(CountdownRunning);
        }

        public override void Read(PacketReader reader)
        {
            base.Read(reader);
            CountdownRunning = reader.ReadBool();
        }

        public override void Handle()
        {
            if (CountdownRunning)
                SceneLobby.StartGameCountdown();
            else
                SceneLobby.CancelGameCountdown();
        }
    }
}