using Godot;

namespace GodotModules;

public class UIOptionsAudio : Control
{
    [Export] protected readonly NodePath NodePathMusic;
    [Export] protected readonly NodePath NodePathSFX;

    private HSlider _music;
    private HSlider _sfx;

    private MusicManager _musicManager;
    private OptionsManager _optionsManager;

    public void PreInit(MusicManager musicManager, OptionsManager optionsManager) 
    {
        _musicManager = musicManager;
        _optionsManager = optionsManager;
    }

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