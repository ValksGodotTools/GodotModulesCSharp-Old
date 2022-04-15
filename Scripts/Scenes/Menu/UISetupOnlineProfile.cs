using Godot;

namespace GodotModules.Netcode
{
    public class UISetupOnlineProfile : AcceptDialog
    {
        [Export] public readonly NodePath NodePathInputUsername;
        [Export] public readonly NodePath NodePathLabelFeedback;

        public LineEdit InputUsername { get; private set; }
        public Label LabelFeedback { get; private set; }

        public override void _Ready()
        {
            InputUsername = GetNode<LineEdit>(NodePathInputUsername);
            LabelFeedback = GetNode<Label>(NodePathLabelFeedback);
        }
    }
}