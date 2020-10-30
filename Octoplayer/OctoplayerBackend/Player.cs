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
        private MediaPlayer media;
        private DispatcherTimer trackTimer;
        public Queue Queue { get; private set; }
        public bool IsPlaying { get; private set; }
        public double CurrentTrackLength
        {
            get 
            { 
                if (!media.NaturalDuration.HasTimeSpan) return 0;
                return media.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
        }
        public double CurrentTrackPosition
        {
            get
            {
                return media.Position.TotalMilliseconds;
            }
            set
            {
                this.media.Position = TimeSpan.FromMilliseconds(value);
            }
        }
        public delegate void EmptyHandler(); 
        public event EmptyHandler MediaPlaying;
        public event EmptyHandler MediaPaused;
        public event EmptyHandler TrackLoaded;
        public event EmptyHandler QueueUpdated;
        
        public Player()
        {
            this.media = new MediaPlayer();
            //this.Queue = new Queue(new List<Track>(), 0, false);
            this.media.MediaOpened += OnTrackLoad;
            this.media.MediaEnded += OnTrackEnd;
            this.IsPlaying = false;
            
        }

        public void SelectTracks(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Queue = new Queue(tracks, startPos, shuffle);
            LoadTrack();
        }

        public void LoadTrack()
        {
            media.Open(new Uri(Queue.CurrentTrack.FilePath));
            trackTimer = new DispatcherTimer();
            QueueUpdated();
            if (IsPlaying) media.Play();
        }

        public void Suspend()
        {
            media.Pause();
            trackTimer.Stop();
        }

        public void Unsuspend()
        {
            media.Play();
            trackTimer.Start();
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
            QueueUpdated();
        }

        public void Previous()
        {
            if (CurrentTrackPosition > 5000) CurrentTrackPosition = 0;
            else
            {
                Queue.Previous();
                LoadTrack();
                QueueUpdated();
            }
        }

        public void SkipTo(int position)
        {
            Queue.SkipTo(position);
            LoadTrack();
        }

        public void ShuffleQueue()
        {
            if(this.Queue != null)
            {
                this.Queue.Shuffle();
                QueueUpdated();
            }
        }

        public void UnshuffleQueue()
        {
            if (this.Queue != null)
            {
                this.Queue.Unshuffle();
                QueueUpdated();
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
