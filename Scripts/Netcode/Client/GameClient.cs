using ENet;

namespace GodotModules.Netcode.Client
{
    public class GameClient : ENetClient
    {
        public Dictionary<uint, string> Players { get; set; }

        public GameClient()
        {
            Players = new();
        }

        protected override void Connect(Event netEvent)
        {
            Log("Client connected to server");
        }

        protected override void Timeout(Event netEvent)
        {
            Log("Client connection timeout");
        }

        protected override void Disconnect(Event netEvent)
        {
            Log("Client disconnected from server");
        }
    }
}