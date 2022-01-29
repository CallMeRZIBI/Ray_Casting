﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;

namespace RayCasting.Sound
{
    class WindowsSound : IPlayer
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder stringReturn, int returnLength, IntPtr hwndCallback);
        private readonly string _path;
        private Timer _playbackTimer;
        private Stopwatch _playStopwatch;
        public event EventHandler PlaybackFinished;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }

        public WindowsSound(string path)
        {
            _path = path;
            Playing = false;
            Paused = false;
        }

        ~WindowsSound()
        {
            ExecuteMsiCommand("Close All");
        }

        public Task Pause()
        {
            if(Playing && !Paused)
            {
                ExecuteMsiCommand($"Pause {_path}");
                Paused = true;
                _playbackTimer.Stop();
                _playStopwatch.Stop();
                _playbackTimer.Interval -= _playStopwatch.ElapsedMilliseconds;
            }

            return Task.CompletedTask;
        }

        public Task Play()
        {
            _playbackTimer = new Timer
            {
                AutoReset = false
            };
            _playStopwatch = new Stopwatch();
            ExecuteMsiCommand("Close All");
            ExecuteMsiCommand($"Status {_path} Length");
            ExecuteMsiCommand($"Play {_path}");
            Paused = false;
            Playing = true;
            _playbackTimer.Elapsed += HandlePlaybackFinished;
            _playbackTimer.Start();
            _playStopwatch.Start();

            return Task.CompletedTask;
        }

        public Task Resume()
        {
            if(Playing && Paused)
            {
                ExecuteMsiCommand($"Resume {_path}");
                Paused = false;
                _playbackTimer.Start();
                _playStopwatch.Reset();
                _playStopwatch.Start();
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if(Playing)
            {
                ExecuteMsiCommand($"Stop {_path}");
                Playing = false;
                Paused = false;
                _playbackTimer.Stop();
                _playStopwatch.Stop();
            }

            return Task.CompletedTask;
        }

        private void ExecuteMsiCommand(string commandString)
        {
            var result = mciSendString(commandString, null, 0, IntPtr.Zero);

            if(result != 0)
            {
                throw new Exception($"Error executing MSI command. Error code: {result}");
            }
        }

        private void HandlePlaybackFinished(object sender, ElapsedEventArgs e)
        {
            Playing = false;
            PlaybackFinished?.Invoke(this, e);
            _playbackTimer.Dispose();
            _playbackTimer = null;
        }
    }
}
