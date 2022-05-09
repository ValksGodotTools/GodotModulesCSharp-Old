using Godot;
using System;

namespace GodotModules.Netcode.Server
{
    public class ServerSimulation : Node
    {
        private static ConcurrentQueue<ThreadCmd<SimulationOpcode>> ServerSimulationQueue { get; set; }

        public override void _Ready()
        {
            ServerSimulationQueue = new ConcurrentQueue<ThreadCmd<SimulationOpcode>>();
        }

        public static void Dequeue()
        {
            if (ServerSimulationQueue.TryDequeue(out ThreadCmd<SimulationOpcode> cmd))
            {
                /*switch (cmd.Opcode)
                {
                    case GodotOpcode.LogMessage:
                        break;
                }*/
            }
        }
    }
}
