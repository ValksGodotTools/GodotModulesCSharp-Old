using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules.Netcode 
{
    public class NetworkManager 
    {
        public DateTime PingSent { get; set; }
        public DisconnectOpcode DisconnectOpcode { get; set; }
        public GameClient Client { get; set; }
        public GameServer Server { get; set; }

        private readonly GodotCommands _godotCmds;
        private readonly bool _enetInitialized;
        
        public NetworkManager()
        {
            _godotCmds = new(this);

            Client = new(this, _godotCmds);
            Server = new(this);
            _enetInitialized = ENet.Library.Initialize();
            if (!_enetInitialized) 
            {
                Logger.LogWarning("Failed to initialize ENet! Remember ENet-CSharp.dll and enet.dll are required in order for ENet to run properly!");
                return;
            }
        }

        public async Task Update()
        {
            await _godotCmds.Update();
        }

        public async void StartClient(string ip, ushort port)
        {
            Client.Dispose();
            Client = new GameClient(this, _godotCmds);
            await Client.StartAsync(ip, port);
        }

        public async void StartServer(ushort port, int maxPlayers)
        {
            Server.Dispose();
            Server = new GameServer(this);
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

            if (_enetInitialized)
                ENet.Library.Deinitialize();
        }
    }
}