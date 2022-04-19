using Godot;
using System;

namespace GodotModules
{
    public class UIPopupError : WindowDialog
    {
        [Export] public readonly NodePath NodePathStackTrace;

        private TextEdit StackTrace { get; set; }

        public void Init(string errorMessage, string stackTrace)
        {
            StackTrace = GetNode<TextEdit>(NodePathStackTrace);
            WindowTitle = errorMessage;
            StackTrace.Text = stackTrace;
        }

        private void _on_Ok_pressed()
        {
            Hide();
        }
    }
}

