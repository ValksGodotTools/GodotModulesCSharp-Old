namespace GodotModules
{
    public class CommandExit : Command
    {
        public CommandExit() => Aliases = new string[] { "stop" };

        public override void Run(string[] args)
        {
            GM.Exit();
        }
    }
}