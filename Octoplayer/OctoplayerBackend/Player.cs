using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace OctoplayerBackend
{
    public class Player
    {
        public MediaPlayer Media;
        public DispatcherTimer TrackTimer;
        public Queue Queue;
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
        public delegate void EmptyHandler(); 
        public event EmptyHandler MediaPlaying;
        public event EmptyHandler MediaPaused;
        public event EmptyHandler TrackLoaded;
        
        public Player()
        {
            this.Media = new MediaPlayer();
            this.Media.MediaOpened += OnTrackLoad;
            this.Media.MediaEnded += OnTrackEnd;
            this.IsPlaying = false;
        }

        public void SelectTracks(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Queue = new Queue(tracks, startPos, shuffle);
            LoadTrack();
        }

        public void LoadTrack()
        {
            Media.Open(new Uri(Queue.CurrentTrack.FilePath));
            TrackTimer = new DispatcherTimer();
            if (IsPlaying) Media.Play();
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

        public void Play()
        {
            Unsuspend();
            IsPlaying = true;
            MediaPlaying();
        }

        public void Pause()
        {
            Suspend();
            IsPlaying = false;
            MediaPaused();
        }

        public void Next()
        {
            Queue.Next();
            LoadTrack();
        }

        public void Previous()
        {
            if (CurrentTrackPosition > 5000) CurrentTrackPosition = 0;
            else
            {
                Queue.Previous();
                LoadTrack();
            }
        }

        public void ShuffleQueue()
        {
            if(this.Queue != null)
            {
                this.Queue.Shuffle();
            }
        }

        public void UnshuffleQueue()
        {
            if (this.Queue != null)
            {
                this.Queue.Unshuffle();
            }
        }

        private void OnTrackLoad(object sender, EventArgs e)
        {
            TrackLoaded();
        }

        private void OnTrackEnd(object sender, EventArgs e)
        {
            Next();
        }
    }
}
