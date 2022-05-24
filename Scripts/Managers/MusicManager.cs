namespace GodotModules 
{
    public class MusicManager 
    {
        private Dictionary<string, AudioStream> _tracks = new();
        private AudioStreamPlayer _audioStreamPlayer;
        private OptionsManager _optionsManager;

        public MusicManager(AudioStreamPlayer audioStreamPlayer, OptionsManager optionsManager)
        {
            _audioStreamPlayer = audioStreamPlayer;
            _optionsManager = optionsManager;
            _audioStreamPlayer.VolumeDb = optionsManager.Options.MusicVolume;
        }

        public void LoadTrack(string name, string path) =>
            _tracks[name.ToLower()] = ResourceLoader.Load<AudioStream>($"res://{path}");

        public void PlayTrack(string name)
        {
            _audioStreamPlayer.Stream = _tracks[name.ToLower()];
            _audioStreamPlayer.Playing = true;
        }

        public void SetVolume(float volume) => SetVolumeValue(volume.Remap(0, 100, -40, 0)); // expects a value from 0 to 100

        public void SetVolumeValue(float volume) // expects a value from -40 to 0
        {
            if (volume <= -40)
                volume = -80; // can't go lower than this (this essentially mutes the track)

            _audioStreamPlayer.VolumeDb = volume;
            _optionsManager.Options.MusicVolume = volume;
        }

        public void Pause() => _audioStreamPlayer.Playing = false;
        public void Resume() => _audioStreamPlayer.Playing = true;
    }
}