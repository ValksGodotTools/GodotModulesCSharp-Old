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
            if (args.Length == 0)
                return;

            if (!int.TryParse(args[0], out int result))
                return;

            SceneGame.Test = result;
            Utils.Log("updated test to " + result);
        }
    }
}