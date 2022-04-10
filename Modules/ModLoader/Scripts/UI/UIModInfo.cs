using Godot;

namespace Valk.ModLoader
{
    public class UIModInfo : Control
    {
        [Export] public readonly NodePath NodePathLabelModName;
        [Export] public readonly NodePath NodePathBtnModEnabled;

        public Label LabelModName { get; set; }
        public Button BtnModEnabled { get; set; }

        // This mod info is displayed on the right side in the dependency mod list
        public bool DisplayedInDependencies { get; set; } 

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

        public void SetColor(Color color) 
        {
            LabelModName.AddColorOverride("font_color", color);
            BtnModEnabled.AddColorOverride("font_color", color);
        }

        private void _on_Enabled_pressed()
        {
            var enabled = !BtnModEnabled.Pressed;
            var modName = LabelModName.Text;

            BtnModEnabled.Text = enabled ? "[x]" : "[ ]";
            ModLoader.ModsEnabled[modName] = enabled;

            if (DisplayedInDependencies)
                UIModLoader.ModInfoList[modName].SetModEnabled(enabled);
            else 
                if (UIModLoader.ModInfoDependencyList.ContainsKey(modName))
                    UIModLoader.ModInfoDependencyList[modName].SetModEnabled(enabled);
        }

        private void _on_PanelContainer_gui_input(InputEvent e)
        {
            if (Input.IsActionPressed("ui_left_click"))
                UIModLoader.UpdateModInfo(LabelModName.Text);
        }
    }
}
