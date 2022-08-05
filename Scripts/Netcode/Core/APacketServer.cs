using GodotModules.Netcode.Client;

namespace GodotModules.Netcode;

public abstract class APacketServer : APacket
{
    /// <summary>
    /// The packet handled client-side
    /// </summary>
    public virtual Task Handle(GameClient client, Managers managers) => Task.FromResult(1);
}
