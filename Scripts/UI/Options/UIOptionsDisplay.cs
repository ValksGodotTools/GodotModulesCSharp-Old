using Godot;

namespace GodotModules
{
    public class UIOptionsDisplay : Control
    {
        [Export] public readonly NodePath NodePathFullscreen;
        private OptionButton _fullscreen;
        private OptionsManager _optionsManager;

        public void PreInit(OptionsManager optionsManager)
        {
            _optionsManager = optionsManager;
        }

        public override void _Ready()
        {
            _fullscreen = GetNode<OptionButton>(NodePathFullscreen);
            _fullscreen.AddItem("Windowed");
            _fullscreen.AddItem("Borderless");
            _fullscreen.AddItem("Exclusive Fullscreen");
        }

        private void _on_VSync_toggled(bool v)
        {
            _optionsManager.SetVSync(v);
        }

        private void _on_Fullscreen_item_selected(int v)
        {
            _optionsManager.SetFullscreenMode((FullscreenMode)v);
        }
    }

    public enum FullscreenMode
    {
        Windowed,
        Borderless,
        Fullscreen
    }
}

