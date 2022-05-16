namespace GodotModules
{
    public class CommandExit : Command
    {
        public CommandExit() => Aliases = new string[] { "stop" };

        public override void Run(string[] args)
        {
            Logger.LogTodo("This has not been implemented yet");
        }
    }
}