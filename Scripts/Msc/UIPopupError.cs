using Godot;
using System;

namespace GodotModules
{
    public class UIPopupError : WindowDialog
    {
        [Export] public readonly NodePath NodePathStackTrace;

        private TextEdit StackTrace { get; set; }

        public override void _Ready()
        {
            StackTrace = GetNode<TextEdit>(NodePathStackTrace);
        }

        public void Init(string errorMessage, string stackTrace)
        {
            WindowTitle = errorMessage;
            StackTrace.Text = stackTrace;
        }

        private void _on_Ok_pressed()
        {
            Hide();
        }
    }
}

