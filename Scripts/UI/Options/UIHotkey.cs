namespace GodotModules
{
	public class UIHotkey : Node
	{
		[Export] protected readonly NodePath NodePathLabel;
		[Export] protected readonly NodePath NodePathBtnHotkey;

		private Label _label;
		private Button _btn;

		private HotkeyManager _hotkeyManager;
		private string _action;
		private string _hotkey;
		private bool _waitingForHotkey;
        private bool _skip;

		public void Init(HotkeyManager hotkeyManager, string action, string hotkey)
		{
			_hotkeyManager = hotkeyManager;
			_action = action;
			_hotkey = hotkey;
		}

		public override void _Ready()
		{
			_label = GetNode<Label>(NodePathLabel);
			_btn = GetNode<Button>(NodePathBtnHotkey);
			_label.Text = _action.Replace("_", " ").ToTitleCase().SmallWordsToUpper(2, (word) => {
				var words = new string[] {"Up", "In"};
				return !words.Contains(word);
			});
			_btn.Text = _hotkey;
		}

		public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && !keyEvent.Pressed && _waitingForHotkey)
            {
                _waitingForHotkey = false;
                SetHotkeyText(@event);
                _hotkeyManager.SetHotkey(_action, keyEvent);
            }

            if (@event is InputEventJoypadButton joypadButtonEvent && !joypadButtonEvent.Pressed && _waitingForHotkey)
            {
                _waitingForHotkey = false;
                SetHotkeyText(@event);
                _hotkeyManager.SetHotkey(_action, joypadButtonEvent);
            }

            if (@event is InputEventMouseButton mouseEvent)
            {
                if (
                    mouseEvent.Pressed && 
                    mouseEvent.ButtonMask == (int)ButtonList.Left && 
                    !_btn.GetGlobalRect().HasPoint(mouseEvent.Position)
                )
                {
                    LostFocus();
                    return;
                }

                if (!mouseEvent.Pressed && _waitingForHotkey)
                {
                    _waitingForHotkey = false;
                    SetHotkeyText(@event);
                    _hotkeyManager.SetHotkey(_action, mouseEvent);
                    _skip = true;
                }
            }
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
            _btn.Text = "...";
		}

		private void _on_Btn_focus_exited() => LostFocus();

		private void _on_Reset_To_Default_pressed() 
		{
			_hotkeyManager.ResetHotkey(_action);
			SetHotkeyText(_hotkeyManager.DefaultHotkeys[_action].InputEventInfo[0].Display());
		}

		public void SetHotkeyText(InputEvent inputEvent) 
        {
			var info = _hotkeyManager.ConvertToInputEventInfo(inputEvent);
			var readable = info.Display();
            SetHotkeyText(readable);
        }

		public void SetHotkeyText(string v)
		{
			_hotkey = v;
            _btn.Text = v;
		}

		private void LostFocus() 
        {
            _waitingForHotkey = false;
            _btn.Text = _hotkey;
        }
	}
}
