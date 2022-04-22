using Godot;
using System;

public class UIDebugger : Control
{
    [Export] public readonly NodePath NodePathLogs;

    private static TextEdit Logs { get; set; }
    private static UIDebugger Instance { get; set; }

    public override void _Ready()
    {
        Instance = this;
        Logs = GetNode<TextEdit>(NodePathLogs);    
    }

    public static void AddException(Exception e)
    {
        Logs.Text += $"{e.Message}\n{e.StackTrace}\n\n";
        Logs.ScrollVertical = Mathf.Inf;
    }
 
    public static void ToggleVisibility()
    {
        Instance.Visible = !Instance.Visible;
    }
}
