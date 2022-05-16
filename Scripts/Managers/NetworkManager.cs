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
        public bool EnetInitialized { get; }

        private readonly GodotCommands _godotCmds;
        
        public NetworkManager()
        {
            _godotCmds = new(this);

            Client = new(this, _godotCmds);
            Server = new(this);

            try 
            {
                EnetInitialized = ENet.Library.Initialize();
            }
            catch (DllNotFoundException) 
            {
                Logger.LogWarning("ENet failed to initialize because enet.dll was not found. Please restart the game and make sure enet.dll is right next to the games executable. Because ENet failed to initialize multiplayer has been disabled.");
                return;
            }

            if (!EnetInitialized) 
                Logger.LogWarning("Failed to initialize ENet! Remember ENet-CSharp.dll and enet.dll are required in order for ENet to run properly!");
        }

        public async Task Update()
        {
            await _godotCmds.Update();
        }

        public async void StartClient(string ip, ushort port)
        {
            if (!EnetInitialized) 
            {
                Logger.LogWarning("Tried to start client but ENet was not initialized properly");
                return;
            }
            
            Client.Dispose();
            Client = new GameClient(this, _godotCmds);
            await Client.StartAsync(ip, port);
        }

        public async void StartServer(ushort port, int maxPlayers)
        {
            if (!EnetInitialized) 
            {
                Logger.LogWarning("Tried to start server but ENet was not initialized properly");
                return;
            }

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

            if (EnetInitialized)
                ENet.Library.Deinitialize();
        }
    }
}