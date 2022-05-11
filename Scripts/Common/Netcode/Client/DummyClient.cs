namespace GodotModules.Netcode.Client 
{
    public class DummyClient : ENetClient 
    {
        protected override void Sent(ClientPacketOpcode opcode)
        {
            GM.Net.PingSent = DateTime.Now;
        }
    }
}