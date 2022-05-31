namespace GodotModules
{
    public class UIOptionsAudio : Control
    {
        [Export] protected readonly NodePath NodePathMusic;
        [Export] protected readonly NodePath NodePathSFX;

        private HSlider _music;
        private HSlider _sfx;

        private Music _musicManager;
        private Options _optionsManager;

        public void PreInit(Music musicManager, Options optionsManager) 
        {
            _musicManager = musicManager;
            _optionsManager = optionsManager;
        }

        public override void _Ready()
        {
            _music = GetNode<HSlider>(NodePathMusic);
            _sfx = GetNode<HSlider>(NodePathSFX);
            _music.Value = _optionsManager.Data.MusicVolume;
            _sfx.Value = _optionsManager.Data.SFXVolume;
        }

        private void _on_Music_value_changed(float v) => _musicManager.SetVolumeValue(v);
        private void _on_SFX_value_changed(float v) {} // TODO
    }
}
