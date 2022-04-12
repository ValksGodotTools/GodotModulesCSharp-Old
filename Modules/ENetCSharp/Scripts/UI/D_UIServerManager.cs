using Godot;
using System;

public class D_UIServerManager : Node
{
    [Export] public readonly NodePath NodePathIp;
    [Export] public readonly NodePath NodePathLogger;

    private static LineEdit InputIp { get; set; }
    private static RichTextLabel Logger { get; set; }

    public override void _Ready()
    {
        InputIp = GetNode<LineEdit>(NodePathIp);
        Logger = GetNode<RichTextLabel>(NodePathLogger);
    }

    public static void Log(string text)
    {
        Logger.AddText($"{text}\n");
    }

    private void _on_Start_pressed()
    {

    }

    private void _on_Stop_pressed()
    {

    }

    private void _on_Restart_pressed()
    {

    }

    private void _on_Connect_pressed()
    {
        var ip = InputIp.Text;
    }

    private void _on_Disconnect_pressed()
    {

    }
}
