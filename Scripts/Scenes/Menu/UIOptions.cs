using Godot;

namespace GodotModules.Settings
{
    public class UIOptions : Node
    {
        public static UIOptions Instance { get; set; }

        [Export] public readonly NodePath NodePathFullscreenOptions;
        [Export] public readonly NodePath NodePathResolutionOptions;
        [Export] public readonly NodePath NodePathSliderMusic;
        [Export] public readonly NodePath NodePathSliderSFX;
        [Export] public readonly NodePath NodePathVSync;
        [Export] public readonly NodePath NodePathInputOnlineUsername;
        [Export] public readonly NodePath NodePathInputWebServerIp;

        public HSlider SliderMusic { get; set; }
        public HSlider SliderSFX { get; set; }
        private OptionButton FullscreenOptions { get; set; }
        private OptionButton ResolutionOptions { get; set; }
        private CheckBox VSync { get; set; }
        public LineEdit InputUsername { get; set; }
        private LineEdit InputWebServer { get; set; }

        public OptionsData Options { get; set; }

        public override void _Ready()
        {
            Instance = this;
            Options = GM.Options;
            SliderMusic = GetNode<HSlider>(NodePathSliderMusic);
            SliderSFX = GetNode<HSlider>(NodePathSliderSFX);
            VSync = GetNode<CheckBox>(NodePathVSync);
            InputUsername = GetNode<LineEdit>(NodePathInputOnlineUsername);
            InputWebServer = GetNode<LineEdit>(NodePathInputWebServerIp);
            SetupFullscreenOptions();
            SetupResolutionOptions();
            SetupUI();
        }

        private void SetupUI()
        {
            // update UI elements
            UtilOptions.CurrentResolution = Options.Resolution;
            ResolutionOptions.Select(Options.Resolution);
            FullscreenOptions.Select((int)Options.FullscreenMode);
            SliderMusic.Value = Options.VolumeMusic;
            SliderSFX.Value = Options.VolumeSFX;
            VSync.Pressed = Options.VSync;
            InputUsername.Text = Options.OnlineUsername;
            InputWebServer.Text = NetworkManager.WebClient.WEB_SERVER_IP;
        }

        private void SetupFullscreenOptions()
        {
            FullscreenOptions = GetNode<OptionButton>(NodePathFullscreenOptions);
            FullscreenOptions.AddItem("Windowed");
            FullscreenOptions.AddItem("Borderless");
            FullscreenOptions.AddItem("Exclusive Fullscreen");
        }

        private void SetupResolutionOptions()
        {
            ResolutionOptions = GetNode<OptionButton>(NodePathResolutionOptions);
            UtilOptions.SupportedResolutions.Values.ForEach(res => ResolutionOptions.AddItem($"{res.x} x {res.y}"));
        }

        private void _on_Resolution_item_selected(int index)
        {
            UtilOptions.CurrentResolution = index;
            Options.Resolution = index;

            if (OS.WindowBorderless || OS.WindowFullscreen)
                return;

            OS.WindowSize = UtilOptions.SupportedResolutions[index];
            UtilOptions.CenterWindow();
        }

        private void _on_Fullscreen_item_selected(int index) => UtilOptions.SetFullscreenMode((FullscreenMode)index);

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

        private string previousTextOnlineUsername = "";

        private void _on_OnlineUsername_text_changed(string text)
        {
            Options.OnlineUsername = text.Validate(ref previousTextOnlineUsername, InputUsername, () => text.IsMatch("^[A-Za-z]+$"));
        }

        private void _on_WebServerIp_text_changed(string text)
        {
            NetworkManager.WebClient.WEB_SERVER_IP = text;
        }
    }
}