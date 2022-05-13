using Godot;

namespace GodotModules
{
    public class UIBtnHotkey : Button
    {
        private string _action;
        private string _hotkey = "";
        private bool _waitingForHotkey;
        private HotkeyManager _hotkeyManager;

        public void PreInit(HotkeyManager hotkeyManager, string action)
        {
            _hotkeyManager = hotkeyManager;
            _action = action;
            var key = (InputEvent)InputMap.GetActionList(_action)[0];
            Text = key.AsText();
            _hotkey = key.AsText();
        }

        public override void _Ready()
        {
            FocusMode = FocusModeEnum.None;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && !keyEvent.Pressed && _waitingForHotkey)
            {
                _waitingForHotkey = false;
                _hotkey = @event.AsText();
                Text = @event.AsText();
                _hotkeyManager.SetHotkey(_action, keyEvent);
            }

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                if (!GetGlobalRect().HasPoint(mouseEvent.Position))
                    LostFocus();
        }

        public void SetHotkeyText(string v) 
        {
            _hotkey = v;
            Text = v;
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