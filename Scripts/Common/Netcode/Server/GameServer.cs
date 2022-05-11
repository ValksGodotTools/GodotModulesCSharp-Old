using ENet;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        protected override void Started(ushort port, int maxClients)
        {
            Log($"Server listening on port {port}");
        }

        protected override void Connect(Event netEvent)
        {
            Log($"Client connected with id: {netEvent.Peer.ID}");
        }

        protected override void Disconnect(Event netEvent)
        {
            Log($"Client disconnected with id: {netEvent.Peer.ID}");
        }

        protected override void Timeout(Event netEvent)
        {
            Log($"Client timed out with id: {netEvent.Peer.ID}");
        }

        protected override void Leave(Event netEvent)
        {
            //Log($"Client left with id: {netEvent.Peer.ID}");
        }

        protected override void Stopped()
        {
            Log("Stopped");
        }

        private void Log(object obj) => GM.Log($"[Server]: {obj}", ConsoleColor.Cyan);
    }
}