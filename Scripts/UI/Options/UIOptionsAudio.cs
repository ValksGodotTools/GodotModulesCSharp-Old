using Godot;

namespace GodotModules
{
    public class UIOptionsAudio : Control
    {
        [Export] public readonly NodePath NodePathMusic;
        [Export] public readonly NodePath NodePathSFX;

        private HSlider _music;
        private HSlider _sfx;

        [Inject] private MusicManager _musicManager;
        [Inject] private OptionsManager _optionsManager;

        public override void _Ready()
        {
            _music = GetNode<HSlider>(NodePathMusic);
            _sfx = GetNode<HSlider>(NodePathSFX);
            _music.Value = _optionsManager.Options.MusicVolume;
            _sfx.Value = _optionsManager.Options.SFXVolume;
        }

        private void _on_Music_value_changed(float v) => _musicManager.SetVolumeValue(v);
        private void _on_SFX_value_changed(float v) {} // TODO
    }
}
