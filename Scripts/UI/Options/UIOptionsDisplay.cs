using Godot;

namespace GodotModules
{
    public class UIOptionsDisplay : Control
    {
        [Export] public readonly NodePath NodePathFullscreen;
        private OptionButton _fullscreen;

        public override void _Ready()
        {
            _fullscreen = GetNode<OptionButton>(NodePathFullscreen);
            _fullscreen.AddItem("Windowed");
            _fullscreen.AddItem("Borderless");
            _fullscreen.AddItem("Exclusive Fullscreen");
        }

        private void _on_VSync_toggled(bool v)
        {
            OS.VsyncEnabled = v;
        }

        private void _on_Fullscreen_item_selected(int v)
        {
            SetFullscreenMode((FullscreenMode)v);
        }

        private void SetFullscreenMode(FullscreenMode mode)
        {
            switch (mode)
            {
                case FullscreenMode.Windowed:
                    SetWindowedMode();
                    break;

                case FullscreenMode.Borderless:
                    SetFullscreenBorderless();
                    break;

                case FullscreenMode.Fullscreen:
                    OS.WindowFullscreen = true;
                    break;
            }
        }

        private void SetWindowedMode()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = false;
            OS.WindowSize = OS.GetScreenSize();
            CenterWindow();
        }

        private void CenterWindow() => OS.WindowPosition = OS.GetScreenSize() / 2 - OS.WindowSize / 2;

        private void SetFullscreenBorderless()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = true;
            OS.WindowPosition = new Vector2(0, 0);
            OS.WindowSize = OS.GetScreenSize() + new Vector2(1, 1); // need to add (1, 1) otherwise will act like fullscreen mode (seems like a Godot bug)
        }
    }

    public enum FullscreenMode
    {
        Windowed,
        Borderless,
        Fullscreen
    }
}

