using Godot;
using System;

namespace ModLoader
{
    public class UIModInfo : Control
    {
        [Export] public readonly NodePath NodePathLabelModName;
        [Export] public readonly NodePath NodePathBtnModEnabled;

        public Label LabelModName { get; set; }
        public Button BtnModEnabled { get; set; }

        public void SetModName(string text)
        {
            LabelModName = GetNode<Label>(NodePathLabelModName);
            LabelModName.Text = text;
        }

        public void SetModEnabled(bool enabled)
        {
            BtnModEnabled = GetNode<Button>(NodePathBtnModEnabled);
            BtnModEnabled.Pressed = !enabled; // not sure why but it works
            BtnModEnabled.Text = enabled ? "[x]" : "[ ]";
        }

        private void _on_Enabled_pressed()
        {
            BtnModEnabled.Text = !BtnModEnabled.Pressed ? "[x]" : "[ ]";
            ModLoader.ModsEnabled[LabelModName.Text] = !BtnModEnabled.Pressed;
        }
    }
}
