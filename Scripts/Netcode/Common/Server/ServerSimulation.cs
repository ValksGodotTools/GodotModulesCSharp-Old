using Godot;
using System;

namespace GodotModules.Netcode.Server
{
    public class ServerSimulation : Node
    {
        private static ConcurrentQueue<GodotCmd> ServerSimulationQueue { get; set; }

        public override void _Ready()
        {
            ServerSimulationQueue = new ConcurrentQueue<GodotCmd>();
        }

        public static void Dequeue()
        {
            if (ServerSimulationQueue.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.LogMessage:
                        break;
                }
            }
        }
    }
}
