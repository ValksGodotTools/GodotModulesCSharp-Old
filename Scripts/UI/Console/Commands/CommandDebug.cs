namespace GodotModules;

public class CommandDebug : Command
{
    public CommandDebug() => Aliases = new string[] { "x" };

    public override void Run(string[] args)
    {
        // debug command
        // do debug stuff here
        if (args.Length == 0)
            return;
    }
}