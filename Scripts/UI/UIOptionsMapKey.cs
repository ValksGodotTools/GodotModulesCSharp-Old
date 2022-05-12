using Godot;

namespace GodotModules
{
    public class UIOptionsMapKey : Button
    {
        [Export] public readonly string Action;

        private string _hotkey = "";
        private bool _waitingForHotkey;

        public override void _Ready()
        {
            var key = (InputEvent)InputMap.GetActionList(Action)[0];
            Text = key.AsText();
            _hotkey = key.AsText();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && !keyEvent.Pressed && _waitingForHotkey)
            {
                _waitingForHotkey = false;
                _hotkey = @event.AsText();
                Text = @event.AsText();
                GM.SetHotkey(Action, keyEvent);
            }

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                if (!GetGlobalRect().HasPoint(mouseEvent.Position))
                    LostFocus();
        }

        private void _on_Btn_pressed()
        {
            _waitingForHotkey = true;
            Text = "...";
        }

        private void _on_Btn_focus_exited() => LostFocus();

        private void LostFocus() 
        {
            _waitingForHotkey = false;
            Text = _hotkey;
        }
    }
}