using Godot;

namespace GodotModules
{
    public class UIModBtn : Control
    {
        [Export] protected readonly NodePath NodePathBtnInfo;
        [Export] protected readonly NodePath NodePathBtnEnable;

        private Button _btnInfo;
        private Button _btnEnable;

        private string _modName;
        private SceneMods _sceneMods;

        public void PreInit(SceneMods sceneMods)
        {
            _sceneMods = sceneMods;
        }

        public override void _Ready()
        {
            _btnInfo = GetNode<Button>(NodePathBtnInfo);
            _btnEnable = GetNode<Button>(NodePathBtnEnable);
        }

        public void DisableButtons() 
        {
            _btnInfo.Disabled = true;
            _btnEnable.Disabled = true;
        }

        public void SetModName(string name) 
        {
            _modName = name;
            _btnInfo.Text = name;
        }

        public void SetEnabled(bool enabled)
        {
            _btnEnable.Pressed = enabled;
            _btnEnable.Text = enabled ? "[x]" : "[  ]";
        }

        public void SetInfo()
        {
            var mods = ModLoader.Mods;

            if (!mods.ContainsKey(_modName))
                return;

            var info = mods[_modName].ModInfo;
            _sceneMods.ModName.Text = _modName;
            _sceneMods.GameVersions.Text = info.GameVersions.Print();
            
            foreach (Control child in _sceneMods.DependencyList.GetChildren())
                child.QueueFree();

            _sceneMods.ModBtnsRight.Clear();

            foreach (var dependency in info.Dependencies.ToList().OrderBy(x => x)) 
            {
                var modBtn = Prefabs.UIModBtn.Instance<UIModBtn>();
                modBtn.PreInit(_sceneMods);
                _sceneMods.DependencyList.AddChild(modBtn);
                _sceneMods.ModBtnsRight.Add(dependency, modBtn);
                modBtn.SetModName(dependency);

                if (mods.ContainsKey(dependency))
                {
                    var modEnabled = mods[dependency].ModInfo.Enabled;
                    modBtn.SetEnabled(modEnabled);
                }
                else 
                {
                    modBtn.SetEnabled(false);
                    modBtn.DisableButtons();
                }
            }
            _sceneMods.Description.Text = info.Description;
        }

        private void _on_Info_pressed()
        {
            SetInfo();
        }

        private void _on_Enable_toggled(bool enabled)
        {
            if (enabled) 
            {
                if (_sceneMods.ModBtnsRight.ContainsKey(_modName)) _sceneMods.ModBtnsRight[_modName].SetEnabled(true);
                if (_sceneMods.ModBtnsLeft.ContainsKey(_modName)) _sceneMods.ModBtnsLeft[_modName].SetEnabled(true);
                ModLoader.EnableMod(_modName);
            }
            else 
            {
                if (_sceneMods.ModBtnsRight.ContainsKey(_modName)) _sceneMods.ModBtnsRight[_modName].SetEnabled(false);
                if (_sceneMods.ModBtnsLeft.ContainsKey(_modName)) _sceneMods.ModBtnsLeft[_modName].SetEnabled(false);
                ModLoader.DisableMod(_modName);
            }
        }
    }
}
