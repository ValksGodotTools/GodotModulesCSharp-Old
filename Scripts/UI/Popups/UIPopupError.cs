using Godot;
using System;

public class UIPopupError : WindowDialog
{
    [Export] public readonly NodePath NodePathError;

    private string _message;
    private string _title;

    public void PreInit(Exception exception, string title = "") 
    {
        _message = exception.StackTrace;
        if (!string.IsNullOrWhiteSpace(title))
            _title = title;
        else
            _title = exception.Message;
    }

    public override void _Ready()
    {
        WindowTitle = _title;
        GetNode<TextEdit>(NodePathError).Text = _message;
    }

    private void _on_UIPopupError_popup_hide() => QueueFree();

    private void _on_Ok_pressed() => QueueFree();
}
