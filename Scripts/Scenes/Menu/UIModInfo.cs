using Godot;

namespace GodotModules.ModLoader
{
    public class UIModInfo : Control
    {
        [Export] public readonly NodePath NodePathBtnMod;
        [Export] public readonly NodePath NodePathBtnModEnabled;

        public Button BtnMod { get; set; }
        public Button BtnModEnabled { get; set; }

        // This mod info is displayed on the right side in the dependency mod list
        public bool DisplayedInDependencies { get; set; }

        public string ModName { get; set; }
        public bool Enabled { get; set; }

        public void SetModName(string text)
        {
            BtnMod = GetNode<Button>(NodePathBtnMod);
            BtnMod.Text = text;
            ModName = text;
        }

        public void SetModEnabled(bool enabled)
        {
            BtnModEnabled = GetNode<Button>(NodePathBtnModEnabled);
            BtnModEnabled.Pressed = !enabled; // not sure why but it works
            BtnModEnabled.Text = enabled ? "[x]" : "[ ]";
            Enabled = enabled;
        }

        public void SetColor(Color color)
        {
            BtnMod.AddColorOverride("font_color", color);
            BtnModEnabled.AddColorOverride("font_color", color);
        }

        private void _on_Enabled_pressed()
        {
            Enabled = !BtnModEnabled.Pressed;
            var modName = BtnMod.Text;

            BtnModEnabled.Text = Enabled ? "[x]" : "[ ]";
            ModLoader.ModInfo[modName].ModInfo.Enabled = Enabled;

            if (DisplayedInDependencies)
                UIModLoader.Instance.ModInfoList[modName].SetModEnabled(Enabled);
            else
                if (UIModLoader.Instance.ModInfoDependencyList.ContainsKey(modName))
                UIModLoader.Instance.ModInfoDependencyList[modName].SetModEnabled(Enabled);
        }

        private void _on_Mod_pressed() => UIModLoader.Instance.UpdateModInfo(BtnMod.Text);
    }
}