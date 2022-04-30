using Godot;
using System;

namespace GodotModules
{
    public class UIPopupMessage : WindowDialog
    {
        [Export] public readonly NodePath NodePathMessage;

        private Label Message { get; set; }

        public override void _Ready()
        {
            Message = GetNode<Label>(NodePathMessage);
        }

        public void Init(string message)
        {
            Message.Text = message;
        }

        private void _on_Ok_pressed()
        {
            Hide();
            QueueFree();
        }
    }
}

