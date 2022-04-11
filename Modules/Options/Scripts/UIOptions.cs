using Godot;
using System;

namespace Valk.Modules.Settings
{
    public class UIOptions : Node
    {
        [Export] public readonly NodePath NodePathFullscreenOptions;
        [Export] public readonly NodePath NodePathSliderMusic;
        [Export] public readonly NodePath NodePathVSync;

        public static HSlider SliderMusic { get; set; }
        private static OptionButton FullscreenOptions { get; set; }
        private static CheckBox VSync { get; set; }
        private static UIOptions Instance { get; set; }
        public static Options Options { get; set; }

        public override void _Ready()
        {
            Instance = this;
            SliderMusic = GetNode<HSlider>(NodePathSliderMusic);
            VSync = GetNode<CheckBox>(NodePathVSync);
            SetupFullscreenOptions();
            ApplyOptions();
        }

        private static void ApplyOptions()
        {
            Options = FileManager.GetConfig<Options>(PathOptions);

            if (Options.FullscreenMode != FullscreenMode.Windowed) 
            {
                Instance.SetFullscreenMode(Options.FullscreenMode);
                FullscreenOptions.Select((int)Options.FullscreenMode);
            }

            MusicManager.SetVolumeValue(Options.MusicVolume);
            SliderMusic.Value = Options.MusicVolume;

            OS.VsyncEnabled = Options.VSync;
            VSync.Pressed = Options.VSync;
        }

        public static string PathOptions => System.IO.Path.Combine(FileManager.GetProjectPath(), "options.json");
        public static void SaveOptions() => FileManager.WriteConfig(PathOptions, Options);

        private static void SetupFullscreenOptions()
        {
            FullscreenOptions = Instance.GetNode<OptionButton>(Instance.NodePathFullscreenOptions);
            FullscreenOptions.AddItem("Windowed");
            FullscreenOptions.AddItem("Borderless");
            FullscreenOptions.AddItem("Exclusive Fullscreen");

            FullscreenOptions.Connect("item_selected", Instance, nameof(_on_FullscreenMode_item_selected));
        }

        private void _on_FullscreenMode_item_selected(int index) => SetFullscreenMode((FullscreenMode)index);

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

            Options.FullscreenMode = mode;
        }

        private void _on_Music_value_changed(float value) 
        {
            MusicManager.SetVolumeValue(value);
            Options.MusicVolume = value;
        }

        private void _on_VSync_toggled(bool enabled) 
        {
            OS.VsyncEnabled = enabled;
            Options.VSync = enabled;
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
        Borderless,
        Fullscreen
    }

    public class Options 
    {
        public FullscreenMode FullscreenMode { get; set; }
        public float MusicVolume { get; set; }
        public bool VSync { get; set; }
    }
}