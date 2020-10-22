using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace OctoplayerBackend
{
    public class Player
    {
        public MediaPlayer Media;
        public DispatcherTimer TrackTimer;
        public Track CurrentTrack;
        public bool IsPlaying;
        public double CurrentTrackLength
        {
            get 
            { 
                if (!Media.NaturalDuration.HasTimeSpan) return 0;
                return Media.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
        }
        public double CurrentTrackPosition
        {
            get
            {
                return Media.Position.TotalMilliseconds;
            }
            set
            {
                this.Media.Position = TimeSpan.FromMilliseconds(value);
            }
        }

        public delegate void PausePlayHandler();
        public event PausePlayHandler PlayerPlaying;
        public event PausePlayHandler PlayerPaused;
        
        public Player()
        {
            this.Media = new MediaPlayer();
            this.IsPlaying = false;
        }

        public void LoadTrack(Track track)
        {
            CurrentTrack = track;
            Media.Open(new Uri(track.FilePath));
            TrackTimer = new DispatcherTimer();
            if (IsPlaying) Media.Play();
        }

        public void Play()
        {
            Unsuspend();
            IsPlaying = true;
            PlayerPlaying();
        }

        public void Pause()
        {
            Suspend();
            IsPlaying = false;
            PlayerPaused();
        }

        public void Suspend()
        {
            Media.Pause();
            TrackTimer.Stop();
        }

        public void Unsuspend()
        {
            Media.Play();
            TrackTimer.Start();
        }

        
    }
}
