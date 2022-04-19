using Common.Netcode;
using Godot;
using GodotModules.Netcode.Client;
using System;

namespace GodotModules
{
    public class UIDisconnected : Node
    {
        [Export] public readonly NodePath NodePathFeedback;

        private Label Feedback { get; set; }

        public override void _Ready()
        {
            Feedback = GetNode<Label>(NodePathFeedback);

            switch (ENetClient.DisconnectOpcode)
            {
                case DisconnectOpcode.Timeout:
                    Feedback.Text = "Timed out from server";
                    break;
                case DisconnectOpcode.Restarting:
                    Feedback.Text = "Server is restarting..";
                    break;
                case DisconnectOpcode.Stopping:
                    Feedback.Text = "Server was stopped";
                    break;
                case DisconnectOpcode.Kicked:
                    Feedback.Text = "You were kicked";
                    break;
                case DisconnectOpcode.Banned:
                    Feedback.Text = "You were banned";
                    break;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GameManager.ChangeScene("GameServers");
            }
        }
    }

}