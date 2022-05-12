using ENet;

namespace GodotModules.Netcode.Client 
{
    public class GameClient : ENetClient
    {
        public GameClient(GodotCommands godotCmds)
        {
            _godotCmds = godotCmds;
        }

        protected override void Connecting()
        {
            Log("Client connecting...");
        }

        protected override void Connect(ref Event netEvent)
        {
            Log("Client connected");
        }

        protected override void Leave(ref Event netEvent)
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