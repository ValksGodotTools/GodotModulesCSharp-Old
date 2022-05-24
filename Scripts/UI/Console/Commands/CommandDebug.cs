using GodotModules.Netcode.Client;
using GodotModules.Netcode.Server;

namespace GodotModules
{
    public class CommandDebug : Command
    {
        public CommandDebug() => Aliases = new[] { "x" };

        public override void Run(string[] args)
        {
            // debug command
            // do debug stuff here
            if (args.Length == 0)
                return;
        }
    }
}