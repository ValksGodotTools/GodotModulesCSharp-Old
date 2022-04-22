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

    public static void AddException(Exception e) => AddMessage($"{e.Message}\n{e.StackTrace}");

    public static void AddMessage(object message)
    {
        Logs.Text += $"{message}\n";
        ScrollToBottom();
    }
 
    public static void ToggleVisibility() 
    {
        Instance.Visible = !Instance.Visible;
        ScrollToBottom();
    }

    private static void ScrollToBottom() => Logs.ScrollVertical = Mathf.Inf;
}
