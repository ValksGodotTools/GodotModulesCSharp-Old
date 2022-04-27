using Godot;
using System.Collections.Generic;
using GodotModules.Settings;

namespace GodotModules
{
    public class MusicManager : AudioStreamPlayer
    {
        private static Dictionary<string, AudioStream> Tracks { get; set; }
        private static MusicManager Instance { get; set; }

        public override void _Ready()
        {
            Instance = this;
            Tracks = new();
        }

        /// <summary>
        /// Load a music track
        /// </summary>
        /// <param name="name">A friendly name that will be used as a reference later on</param>
        /// <param name="path">
        /// The path to the audio file, note that res:// is included if not specified
        /// </param>
        public static void LoadTrack(string name, string path)
        {
            if (!path.Contains("res://"))
                path = "res://" + path;

            Tracks[name.ToLower()] = ResourceLoader.Load<AudioStream>(path);
        }

        /// <summary>
        /// Plays a music track, if a current track is playing already then that track will be replaced with the new track
        /// </summary>
        /// <param name="name">The name of the music track to play</param>
        public static void PlayTrack(string name)
        {
            Instance.Stream = Tracks[name.ToLower()];
            Instance.Playing = true;
        }

        /// <summary>
        /// Set the volume of the background music
        /// </summary>
        /// <param name="volume">A value ranging from 0 to 100</param>
        /// <returns></returns>
        public static void SetVolume(float volume) => SetVolumeValue(volume.Remap(0, 100, -40, 0));

        /// <summary>
        /// Set the volume of the background music
        /// </summary>
        /// <param name="volume">A value ranging from -40 to 0</param>
        public static void SetVolumeValue(float volume)
        {
            if (volume <= -40)
                volume = -80; // can't go lower than this (this essentially mutes the track)

            Instance.VolumeDb = volume;

            if (UIOptions.Instance != null)
                UIOptions.Instance.SliderMusic.Value = volume;
        }

        /// <summary>
        /// Pause the current music track
        /// </summary>
        public static void Pause() => Instance.Playing = false;

        /// <summary>
        /// Resume the current music track
        /// </summary>
        public static void Resume() => Instance.Playing = true;
    }
}