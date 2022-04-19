using Godot;
using System;

public class UIPopupMessage : WindowDialog
{
    [Export] public readonly NodePath NodePathMessage;

    private Label Message { get; set; }

    public void Init(string message) 
    {
        Message = GetNode<Label>(NodePathMessage);
        Message.Text = message;
    }

    private void _on_Ok_pressed() 
    {
        Hide();
        QueueFree();
    }
}
