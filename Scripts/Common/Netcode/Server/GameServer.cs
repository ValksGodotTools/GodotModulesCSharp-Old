using ENet;

namespace GodotModules.Netcode.Server
{
    public class GameServer : ENetServer
    {
        protected override void ServerCmds()
        {
            while (ENetCmds.TryDequeue(out ENetServerCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case ENetServerOpcode.Stop:
                        if (CancellationTokenSource.IsCancellationRequested) 
                        {
                            Log("Server is in the middle of stopping");
                            break;
                        }

                        KickAll(DisconnectOpcode.Stopping);
                        CancellationTokenSource.Cancel();
                        break;

                    case ENetServerOpcode.Restart:
                        if (CancellationTokenSource.IsCancellationRequested)
                        {
                            Log("Server is in the middle of restarting");
                            break;
                        }

                        KickAll(DisconnectOpcode.Restarting);
                        CancellationTokenSource.Cancel();
                        _queueRestart = true;
                        break;
                }
            }
        }

        protected override void Started(ushort port, int maxClients)
        {
            Log($"Server listening on port {port}");
        }

        protected override void Connect(Event netEvent)
        {
            Log($"Client connected with id: {netEvent.Peer.ID}");
        }

        protected override void Received(ClientPacketOpcode opcode)
        {
            Log($"Received packet: {opcode}");
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
            Log("Server stopped");
        }

        private void Log(object obj) => GM.Log($"[Server]: {obj}", ConsoleColor.Cyan);
    }
}