using Godot;
using System;
using System.Collections.Generic;

namespace Valk.Modules.Settings
{
    public class UIOptions : Node
    {
        [Export] public readonly NodePath NodePathFullscreenOptions;
        [Export] public readonly NodePath NodePathResolutionOptions;
        [Export] public readonly NodePath NodePathSliderMusic;
        [Export] public readonly NodePath NodePathSliderSFX;
        [Export] public readonly NodePath NodePathVSync;

        public static HSlider SliderMusic { get; set; }
        public static HSlider SliderSFX { get; set; }
        private static OptionButton FullscreenOptions { get; set; }
        private static OptionButton ResolutionOptions { get; set; }
        private static CheckBox VSync { get; set; }
        private static UIOptions Instance { get; set; }
        public static Options Options { get; set; }
        private static Dictionary<int, Vector2> SupportedResolutions { get; set; }
        private static int CurrentResolution { get; set; }

        public override void _Ready()
        {
            Instance = this;
            SliderMusic = GetNode<HSlider>(NodePathSliderMusic);
            SliderSFX = GetNode<HSlider>(NodePathSliderSFX);
            VSync = GetNode<CheckBox>(NodePathVSync);
            SetupFullscreenOptions();

            SupportedResolutions = GetSupportedResolutions();

            SetupResolutionOptions();
            ApplyOptions();
        }


        private static void ApplyOptions()
        {
            Options = FileManager.GetConfig<Options>(PathOptions);

            if (GameManager.OptionsCreatedForFirstTime)
            {
                // defaults
                Options = new Options 
                {
                    Resolution = SupportedResolutions.Count - 1,
                    FullscreenMode = 0,
                    VolumeMusic = -15,
                    VolumeSFX = -15,
                    VSync = true
                };
            }

            // update UI elements
            CurrentResolution = Options.Resolution;
            ResolutionOptions.Select(Options.Resolution);
            FullscreenOptions.Select((int)Options.FullscreenMode);
            SliderMusic.Value = Options.VolumeMusic;
            SliderSFX.Value = Options.VolumeSFX;
            VSync.Pressed = Options.VSync;
                
            // apply settings
            if (Options.FullscreenMode == FullscreenMode.Windowed) 
            {
                OS.WindowSize = SupportedResolutions[Options.Resolution];
                CenterWindow();
            }
            else
                Instance.SetFullscreenMode(Options.FullscreenMode);

            MusicManager.SetVolumeValue(Options.VolumeMusic);

            OS.VsyncEnabled = Options.VSync;
        }

        public static string PathOptions => System.IO.Path.Combine(FileManager.GetProjectPath(), "options.json");
        public static void SaveOptions() => FileManager.WriteConfig(PathOptions, Options);

        private static void SetupFullscreenOptions()
        {
            FullscreenOptions = Instance.GetNode<OptionButton>(Instance.NodePathFullscreenOptions);
            FullscreenOptions.AddItem("Windowed");
            FullscreenOptions.AddItem("Borderless");
            FullscreenOptions.AddItem("Exclusive Fullscreen");
        }

        /// <summary>
        /// Godot does not provide any functions to print the list of supported resolutions from Monitor[]
        /// This gets the supported resolutions based off OS.GetScreenSize()
        /// </summary>
        /// <returns>The supported resolutions of the monitor</returns>
        private static Dictionary<int, Vector2> GetSupportedResolutions() 
        {
            var supportedResolutions = new Dictionary<int, Vector2>();

            var maxSize = OS.GetScreenSize();

            var resolutions = new Vector2[] {
                new Vector2(426, 240),
                new Vector2(640, 360),
                new Vector2(854, 480),
                new Vector2(1280, 720),
                new Vector2(1920, 1080),
                new Vector2(2560, 1440),
                new Vector2(3840, 2160),
                new Vector2(7680, 4320)
            };

            for (int i = 0; i < resolutions.Length; i++)
                if (resolutions[i] <= maxSize)
                    supportedResolutions.Add(i, resolutions[i]);

            return supportedResolutions;
        }

        private static void SetupResolutionOptions()
        {
            ResolutionOptions = Instance.GetNode<OptionButton>(Instance.NodePathResolutionOptions);

            foreach (var res in SupportedResolutions.Values)
                ResolutionOptions.AddItem($"{res.x} x {res.y}");
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

            Options.FullscreenMode = mode;
        }

        private void _on_Resolution_item_selected(int index)
        {
            CurrentResolution = index;
            Options.Resolution = index;

            if (OS.WindowBorderless || OS.WindowFullscreen)
                return;

            OS.WindowSize = SupportedResolutions[index];
            CenterWindow();
        }

        private void _on_Fullscreen_item_selected(int index) => SetFullscreenMode((FullscreenMode)index);

        private void _on_Music_value_changed(float value)
        {
            MusicManager.SetVolumeValue(value);
            Options.VolumeMusic = value;
        }

        private void _on_SFX_value_changed(float value)
        {
            // to be implemented
            Options.VolumeSFX = value;
        }

        private void _on_VSync_toggled(bool enabled)
        {
            OS.VsyncEnabled = enabled;
            Options.VSync = enabled;
        }

        private static void CenterWindow() => OS.WindowPosition = OS.GetScreenSize() / 2 - OS.WindowSize / 2;

        private static void SetWindowedMode()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = false;
            OS.WindowSize = SupportedResolutions[CurrentResolution];
            CenterWindow();
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
        public int Resolution { get; set; }
        public float VolumeMusic { get; set; }
        public float VolumeSFX { get; set; }
        public bool VSync { get; set; }
    }
}