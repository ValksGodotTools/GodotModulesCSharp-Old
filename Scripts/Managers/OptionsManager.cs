using Godot;

namespace GodotModules 
{
    public class OptionsManager 
    {
        public OptionsData Options;
        private readonly SystemFileManager _systemFileManager;
        private readonly HotkeyManager _hotkeyManager;

        public OptionsManager(SystemFileManager systemFileManager, HotkeyManager hotkeyManager)
        {
            _systemFileManager = systemFileManager;
            _hotkeyManager = hotkeyManager;
            LoadOptions();
        }

        public void SetVSync(bool v)
        {
            OS.VsyncEnabled = v;
            Options.VSync = v;
        }

        public void SetFullscreenMode(FullscreenMode mode)
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

        private void SetWindowedMode()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = false;
            OS.WindowSize = Options.WindowSize;
            CenterWindow();
        }

        private void SetFullscreenBorderless()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = true;
            OS.WindowPosition = new Vector2(0, 0);
            OS.WindowSize = OS.GetScreenSize() + new Vector2(1, 1); // need to add (1, 1) otherwise will act like fullscreen mode (seems like a Godot bug)
        }

        private void CenterWindow() => OS.WindowPosition = OS.GetScreenSize() / 2 - OS.WindowSize / 2;

        private void LoadOptions()
        {
            if (_systemFileManager.ConfigExists("options"))
                Options = _systemFileManager.ReadConfig<OptionsData>("options");
            else 
            {
                var defaultOptions = new OptionsData {
                    VSync = true,
                    FullscreenMode = FullscreenMode.Borderless,
                    MusicVolume = -20,
                    SFXVolume = -20,
                    WindowSize = OS.WindowSize
                };

                Options = defaultOptions;
            }

            SetVSync(Options.VSync);
            SetFullscreenMode(Options.FullscreenMode);
        }

        public void SaveOptions() 
        {
            _hotkeyManager.SaveHotkeys();

            Options.WindowSize = OS.WindowSize;
            _systemFileManager.WriteConfig("options", Options);
        }
    }

    public class OptionsData 
    {
        public bool VSync { get; set; }
        public FullscreenMode FullscreenMode { get; set; }
        public Vector2 WindowSize { get; set; }
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
    }
}