using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace OctoplayerBackend
{
    public class Queue
    {
        public List<Track> Tracks { get; }
        private LinkedList<Track> PreviousTracks;
        private LinkedList<Track> NextTracks;
        public Track CurrentTrack { get; private set; }
    
        
        public Queue()
        {
            this.Tracks = new List<Track>();
            this.PreviousTracks = new LinkedList<Track>();
            this.NextTracks = new LinkedList<Track>();
        }

        public Queue(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Tracks = tracks;
            this.PreviousTracks = new LinkedList<Track>(tracks.Take(startPos));
            this.CurrentTrack = tracks[startPos];
            this.NextTracks = new LinkedList<Track>(Tracks.Skip(startPos + 1));
            if (shuffle) Shuffle();
        }

        public void AddTrack(Track track, bool addToFront)
        {
            if(track != CurrentTrack)
            {
                if (Tracks.Contains(track))
                {
                    Tracks.Remove(track);
                    PreviousTracks.Remove(track);
                    NextTracks.Remove(track);
                }
                if (addToFront)
                {
                    Tracks.Insert(Tracks.IndexOf(CurrentTrack) + 1, track);
                    NextTracks.AddFirst(track);
                }
                else
                {
                    Tracks.Add(CurrentTrack);
                    NextTracks.AddLast(track);
                }
            }
        }

        public void Shuffle()
        {
            PreviousTracks = new LinkedList<Track>();
            NextTracks = new LinkedList<Track>(Tracks.Where(t => t != CurrentTrack).Shuffle());
        }

        public void Unshuffle()
        {
            PreviousTracks = new LinkedList<Track>(Tracks.Take(Tracks.IndexOf(CurrentTrack)));
            NextTracks = new LinkedList<Track>(Tracks.Skip(Tracks.IndexOf(CurrentTrack) + 1));
        }

        public void SkipTo(int position)
        {
            while(position != 0)
            {
                if(position < 0)
                {
                    Previous();
                    position++;
                }
                else
                {
                    Next();
                    position--;
                }
            }
        }

        public void Next()
        {
            if(NextTracks.Count > 0)
            {
                PreviousTracks.AddLast(CurrentTrack);
                CurrentTrack = NextTracks.First();
                NextTracks.RemoveFirst();
            }
        }

        public void Previous()
        {
            if(PreviousTracks.Count > 0)
            {
                NextTracks.AddFirst(CurrentTrack);
                CurrentTrack = PreviousTracks.Last();
                PreviousTracks.RemoveLast();
            }
        }

        public int TopScrollPosition()
        {
            if (PreviousTracks.Count > 2)
            {
                return -2 + PreviousTracks.Count;
            }
            else return 0;
        }

        public List<QueueItem> GetQueueItems()
        {
            var queue = new List<QueueItem>();
            var index = -(PreviousTracks.Count);
            foreach(var track in PreviousTracks)
            {
                queue.Add(new QueueItem(track, index));
                index++;
            }
            queue.Add(new QueueItem(CurrentTrack, index));
            index++;
            foreach(var track in NextTracks)
            {
                queue.Add(new QueueItem(track, index));
                index++;
            }
            return queue;
        }

    }
}
