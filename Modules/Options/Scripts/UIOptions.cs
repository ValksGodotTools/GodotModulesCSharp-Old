using Godot;
using System;

namespace Valk.Modules.Options
{
    public class UIOptions : Node
    {
        [Export] public readonly NodePath NodePathFullscreenOptions;

        private static OptionButton FullscreenOptions { get; set; }

        private static UIOptions Instance;

        public override void _Ready()
        {
            Instance = this;
            SetupFullscreenOptions();
        }

        private static void SetupFullscreenOptions()
        {
            FullscreenOptions = Instance.GetNode<OptionButton>(Instance.NodePathFullscreenOptions);
            FullscreenOptions.AddItem("Windowed");
            FullscreenOptions.AddItem("Fullscreen");
            FullscreenOptions.AddItem("Borderless");

            FullscreenOptions.Connect("item_selected", Instance, nameof(_on_FullscreenMode_item_selected));
        }

        private void _on_FullscreenMode_item_selected(int index)
        {
            var mode = (FullscreenMode)index;

            switch (mode)
            {
                case FullscreenMode.Windowed:
                    SetWindowedMode();
                    break;
                case FullscreenMode.Fullscreen:
                    OS.WindowFullscreen = true;
                    break;
                case FullscreenMode.Borderless:
                    SetFullscreenBorderless();
                    break;
            }
        }

        private static void SetWindowedMode()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = false;
            OS.WindowSize = OS.GetScreenSize() / 2;
            OS.WindowPosition = OS.GetScreenSize() / 2 - OS.WindowSize / 2;
        }

        private static void SetFullscreenBorderless()
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
        Fullscreen,
        Borderless
    }
}