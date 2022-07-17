namespace GodotModules.Netcode.Client 
{
    public class DummyClient : ENetClient 
    {
        public DummyClient(Managers managers) : base(managers) 
        {}

        protected override void Sent(ClientPacketOpcode opcode)
        {
            _networkManager.PingSent = DateTime.Now;
        }
    }
}