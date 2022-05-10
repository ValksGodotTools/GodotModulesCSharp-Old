using Godot;
using System;

namespace GodotModules
{
    // This is the in game console which appears on pressing F12
    public class UIGameConsole : PanelContainer
    {
        [Export] public readonly NodePath NodePathLogs;
        [Export] public readonly NodePath NodePathConsole;

        private TextEdit Logs { get; set; }
        
        private LineEdit Console { get; set; } // the console input

        public override void _Ready()
        {
            Logs = GetNode<TextEdit>(NodePathLogs);
            Console = GetNode<LineEdit>(NodePathConsole);
        }

        public void AddException(Exception e) => AddMessage($"{e.Message}\n{e.StackTrace}");

        public void AddMessage(string message)
        {
            Logs.Text += $"{message}\n";
            ScrollToBottom();
        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
            Console.GrabFocus();
            ScrollToBottom();
        }

        private void ScrollToBottom() => Logs.ScrollVertical = Mathf.Inf;

        private void _on_Console_text_entered(string text)
        {
            var inputArr = text.Trim().ToLower().Split(' ');
            var cmd = inputArr[0];

            if (string.IsNullOrWhiteSpace(cmd))
                return;

            var command = Command.Instances.FirstOrDefault(x => x.IsMatch(cmd));

            if (command != null)
                command.Run(inputArr.Skip(1).ToArray());
            else
                GM.Logger.Log($"The command '{cmd}' does not exist");

            Console.Clear();
        }
    }
}