using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class NetworkManager 
    {
        public DateTime PingSent { get; set; }
        public DisconnectOpcode DisconnectOpcode { get; set; }
        public bool ENetInitialized { get; set; }
        public GameClient Client = new GameClient();
        public GameServer Server = new GameServer();
        
        public NetworkManager()
        {
            ENetInitialized = ENet.Library.Initialize();
            if (!ENetInitialized) 
            {
                GM.LogWarning("Failed to initialize ENet! Remember ENet-CSharp.dll and enet.dll are required in order for ENet to run properly!");
                return;
            }
        }

        public async void StartClient(string ip, ushort port)
        {
            Client.Dispose();
            Client = new GameClient();
            await Client.StartAsync(ip, port);
        }

        public async void StartServer(ushort port, int maxPlayers)
        {
            Server.Dispose();
            Server = new GameServer();
            await Server.StartAsync(port, maxPlayers);
        }

        public bool IsMultiplayer() => Client.IsRunning || Server.IsRunning;

        public async Task Cleanup()
        {
            if (Client.IsRunning) 
            {
                await Client.StopAsync();
                Client.Dispose();
            }

            if (Server.IsRunning)
            {
                await Server.StopAsync();
                Server.Dispose();
            }

            if (ENetInitialized)
                ENet.Library.Deinitialize();
        }
    }
}