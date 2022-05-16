using Godot;
using System;

public class UIPopupMessage : WindowDialog
{
    [Export] public readonly NodePath NodePathMessage;

    private string _message;
    private string _title;

    public void PreInit(string message, string title = "") 
    {
        _message = message;
        _title = title;
    }

    public override void _Ready()
    {
        WindowTitle = _title;
        GetNode<Label>(NodePathMessage).Text = _message;
    }

    private void _on_UIPopupMessage_popup_hide() => QueueFree();

    private void _on_Ok_pressed() => QueueFree();
}
