using ENet;

namespace GodotModules.Netcode.Client 
{
    public class GameClient : ENetClient
    {
        protected override void Connecting()
        {
            Log("Connecting...");
        }

        protected override void Connect(Event netEvent)
        {
            Log("Connected");
        }

        protected override void Leave(Event netEvent)
        {
            Log("Client left");
        }

        protected override void Stopped() 
        {
            Log("Client stopped");
        }

        private void Log(object v) => GM.Log($"[Client]: {v}", ConsoleColor.DarkGreen);
    }
}