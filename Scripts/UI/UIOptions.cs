using Godot;
using System;

namespace GodotModules
{
    public class UIOptions : Node
    {
        [Export] public readonly NodePath NodePathUIControls;
        private UIControls _uiControls;

        public void Init(HotkeyManager hotkeyManager)
        {
            _uiControls = GetNode<UIControls>(NodePathUIControls);
            _uiControls._hotkeyManager = hotkeyManager;
        }
    }
}
