using Godot;
using System;

namespace GodotModules
{
    // This is the in game console which appears on pressing F12
    public class UIDebugger : Control
    {
        [Export] public readonly NodePath NodePathLogs;
        [Export] public readonly NodePath NodePathConsole;

        public static UIDebugger Instance { get; set; }
        private static TextEdit Logs { get; set; }
        
        private LineEdit Console { get; set; } // the console input

        public override void _Ready()
        {
            Instance = this;
            Logs = GetNode<TextEdit>(NodePathLogs);
            Console = GetNode<LineEdit>(NodePathConsole);
        }

        public static void AddException(Exception e) => AddMessage($"{e.Message}\n{e.StackTrace}");

        public static void AddMessage(string message)
        {
            Logs.Text += $"{message}\n";
            ScrollToBottom();
        }

        public static void ToggleVisibility()
        {
            Instance.Visible = !Instance.Visible;
            Instance.Console.GrabFocus();
            ScrollToBottom();
        }

        private static void ScrollToBottom() => Logs.ScrollVertical = Mathf.Inf;

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
                Logger.Log($"The command '{cmd}' does not exist");

            Console.Clear();
        }
    }
}