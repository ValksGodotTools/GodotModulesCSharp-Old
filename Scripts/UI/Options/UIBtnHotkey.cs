using Godot;

namespace GodotModules;

public class UIBtnHotkey : Button
{
    private string _action;
    private string _hotkey = "";
    private bool _waitingForHotkey;
    private bool _skip;
    private HotkeyManager _hotkeyManager;

    public void PreInit(HotkeyManager hotkeyManager, string action)
    {
        _hotkeyManager = hotkeyManager;
        _action = action;
        var key = (InputEvent)InputMap.GetActionList(_action)[0];
        Text = key.Display();
        _hotkey = key.Display();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && !keyEvent.Pressed && _waitingForHotkey)
        {
            _waitingForHotkey = false;
            SetHotkeyText(@event.Display());
            _hotkeyManager.SetHotkey(_action, keyEvent);
        }

        if (@event is InputEventJoypadButton joypadButtonEvent && !joypadButtonEvent.Pressed && _waitingForHotkey)
        {
            _waitingForHotkey = false;
            SetHotkeyText(@event.Display());
            _hotkeyManager.SetHotkey(_action, joypadButtonEvent);
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (
                mouseEvent.Pressed && 
                mouseEvent.ButtonMask == (int)ButtonList.Left && 
                !GetGlobalRect().HasPoint(mouseEvent.Position)
            )
            {
                LostFocus();
                return;
            }

            if (!mouseEvent.Pressed && _waitingForHotkey)
            {
                _waitingForHotkey = false;
                SetHotkeyText(@event.Display());
                _hotkeyManager.SetHotkey(_action, mouseEvent);
                _skip = true;
            }
        }
    }

    public void SetHotkeyText(string v) 
    {
        _hotkey = v;
        Text = v;
    }

    private void _on_Btn_pressed()
    {
        if (_waitingForHotkey)
            return;

        if (_skip)
        {
            _skip = false;
            return;
        }

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