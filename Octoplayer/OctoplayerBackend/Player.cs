using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;
using System.Diagnostics;

namespace OctoplayerBackend
{
    public class Player
    {
        private MediaPlayer media;
        private Library library;
        private Stopwatch trackTimer;
        private List<Track> SelectedTracks;
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
        
        public Player(Library library)
        {
            this.media = new MediaPlayer();
            this.Queue = new Queue();
            this.SelectedTracks = new List<Track>();
            this.library = library;
            this.media.MediaOpened += OnTrackLoad;
            this.media.MediaEnded += OnTrackEnd;
            this.IsPlaying = false; 
        }

        public void SelectTracks(List<Track> tracks, int startPos, bool shuffle)
        {
            if(Queue.CurrentTrack != null) LogData();
            this.Queue = new Queue(tracks, startPos, shuffle);
            SelectedTracks.Add(Queue.CurrentTrack);
            LoadTrack();
        }

        public void AddTrack(Track track, bool addToFront, bool shuffle)
        {
            if (Queue.CurrentTrack == null) SelectTracks(new List<Track>() { track }, 0, shuffle);
            else
            {
                Queue.AddTrack(track, addToFront);
                SelectedTracks.Add(track);
                QueueUpdated();
            }
        }

        private void LoadTrack()
        {
            media.Open(new Uri(Queue.CurrentTrack.FilePath));
            if (SelectedTracks.Contains(Queue.CurrentTrack))
            {
                library.UpdateTrackRatings(Queue.CurrentTrack, 1);
                SelectedTracks.Remove(Queue.CurrentTrack);
            }
            trackTimer = new Stopwatch();
            QueueUpdated();
            if (IsPlaying) media.Play();
        }

        public void LogData()
        {
            var track = Queue.CurrentTrack;
            if(track != null)
            {
                if (trackTimer.ElapsedMilliseconds > 10000)
                {
                    track.PlayCount++;
                    track.LastPlayed = DateTime.Now;
                }
                var change = -1 + (trackTimer.ElapsedMilliseconds * 2 / CurrentTrackLength);
                library.UpdateTrackRatings(track, change);
            }
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
            LogData();
            Queue.Next();
            LoadTrack();
            QueueUpdated();
        }

        public void Previous()
        {
            if (CurrentTrackPosition > 5000) CurrentTrackPosition = 0;
            else
            {
                LogData();
                Queue.Previous();
                LoadTrack();
                QueueUpdated();
            }
        }

        public void SkipTo(int position)
        {
            LogData();
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
