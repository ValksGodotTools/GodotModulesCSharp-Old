namespace GodotModules.Netcode.Client;

public class DummyClient : ENetClient 
{
    public DummyClient(NetworkManager networkManager) : base(networkManager) 
    {}

    protected override void Sent(ClientPacketOpcode opcode)
    {
        _networkManager.PingSent = DateTime.Now;
    }
}