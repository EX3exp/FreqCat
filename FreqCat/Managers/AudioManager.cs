using System;
using System.Collections.Generic;
using System.IO;


using Serilog;
using NetCoreAudio;

namespace FreqCat.Managers
{


    public class AudioManager
    {
        private Player _player = new();

        private List<string> audioPaths;
        private int _currentFileIndex;

        private bool MainViewModelPlaying;
        public bool IsPlaying => _player.Playing;
        public AudioManager()
        {
            //Log.Debug($"player: {_player}");
            _player.PlaybackFinished += OnPlaybackStopped; // called when playback is stopped
            audioPaths = new List<string>();
            _currentFileIndex = 0;
            MainViewModelPlaying = false;
        }

        public async void PlayAudio(string filePath)
        {
            if (_player is null)
            {
                _player = new Player();
            }
            if (_player.Playing)
            {
                _player.Stop();
            }
            if (File.Exists(filePath))
            {
                Log.Debug($"Playing file: {filePath}");
                await _player.Play(filePath);
            }
            else
            {
                Log.Error($"audio file not found: {filePath}");
            }
        }

        private void OnPlaybackStopped(object? sender, EventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Log.Information("Playback stopped.");

            });
        }

        public void PauseAudio()
        {
            if (_player is not null && _player.Playing)
            {
                _player.Pause();
            }

        }

        public void StopAudio()
        {

            if (_player is not null)
            {
                _player.Stop();
                audioPaths.Clear();
            }

        }
        
    }

}
