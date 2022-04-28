using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;
using System.Collections.Generic;
using System.Linq;
using Game;

namespace GodotModules 
{
    public class CommandDebug : Command 
    {
        public CommandDebug() => Aliases = new string[] { "x" };

        public override void Run(string[] args)
        {
            // debug command
            // do debug stuff here
        }
    }
}