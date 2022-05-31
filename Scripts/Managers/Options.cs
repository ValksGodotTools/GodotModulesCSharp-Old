namespace GodotModules 
{
    public class Options 
    {
        public OptionsData Data;
        private readonly SystemFileManager _systemFileManager;
        private readonly HotkeyManager _hotkeyManager;

        public Options(HotkeyManager hotkeyManager)
        {
            _systemFileManager = new();
            _hotkeyManager = hotkeyManager;
            LoadOptions();
        }

        public void SetVSync(bool v)
        {
            OS.VsyncEnabled = v;
            Data.VSync = v;
        }

        public void ToggleFullscreen()
        {
            if (OS.WindowBorderless)
            {
                SetFullscreenMode(FullscreenMode.Windowed);
            }
            else
            {
                SetFullscreenMode(FullscreenMode.Borderless);
            }
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

            Data.FullscreenMode = mode;
        }

        private void SetWindowedMode()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = false;
            OS.WindowSize = Data.WindowSize;
            CenterWindow();
        }

        private void SetFullscreenBorderless()
        {
            OS.WindowFullscreen = false;
            OS.WindowBorderless = true;
            OS.WindowPosition = new Vector2(0, 0);
            OS.WindowSize = OS.GetScreenSize() + new Vector2(1, 1); // need to add (1, 1) otherwise will act like fullscreen mode (seems like a Godot bug)
        }

        public void CenterWindow() => 
            OS.WindowPosition = OS.GetScreenSize() / 2 - OS.WindowSize / 2;

        private void LoadOptions()
        {
            Data = _systemFileManager.ConfigExists("options")
                ? _systemFileManager.ReadConfig<OptionsData>("options")
                : new OptionsData
                {
                    VSync = true,
                    FullscreenMode = FullscreenMode.Borderless,
                    MusicVolume = -20,
                    SFXVolume = -20,
                    WindowSize = OS.WindowSize,
                    WebServerAddress = "localhost:4000",
                    Colors = new OptionColors {
                        Player = "53ff7e",
                        Enemy = "ff5353",
                        ChatText = "a0a0a0"
                    }
                };

            SetVSync(Data.VSync);
            SetFullscreenMode(Data.FullscreenMode);
        }

        public void SaveOptions() 
        {
            _hotkeyManager.SaveHotkeys();
            Data.WindowSize = OS.WindowSize;
            _systemFileManager.WriteConfig("options", Data);
        }
    }

    public class OptionsData 
    {
        public bool VSync { get; set; }
        public FullscreenMode FullscreenMode { get; set; }
        public Vector2 WindowSize { get; set; }
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
        public string OnlineUsername { get; set; }
        public string WebServerAddress { get; set; }
        public OptionColors Colors { get; set; }
    }

    public struct OptionColors 
    {
        public string Player { get; set; }
        public string Enemy { get; set; }
        public string ChatText { get; set; }
    }

    public enum FullscreenMode
    {
        Windowed,
        Borderless,
        Fullscreen
    }
}