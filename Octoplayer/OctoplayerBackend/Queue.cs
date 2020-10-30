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
    

        public Queue(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Tracks = tracks;
            this.PreviousTracks = new LinkedList<Track>(tracks.Take(startPos));
            this.CurrentTrack = tracks[startPos];
            this.NextTracks = new LinkedList<Track>(Tracks.Skip(startPos + 1));
            if (shuffle) Shuffle();
        }

        //public void AddTrack(Track track, bool addToFront)
        //{
        //    Tracks.Add(track);
        //    if (addToFront)
        //    {
                
        //    }
        //    else 
        //}

        public void Shuffle()
        {
            PreviousTracks = new LinkedList<Track>();
            NextTracks = new LinkedList<Track>(Tracks.Where(t => t != CurrentTrack).Shuffle());

            //queueOrder = queueOrder.Shuffle().ToList();
            //var index = queueOrder.IndexOf(queuePosition);
            //queueOrder = queueOrder.Skip(index).Concat(queueOrder.Take(index)).ToList();
            //queuePosition = 0;
        }

        public void Unshuffle()
        {
            PreviousTracks = new LinkedList<Track>(Tracks.Take(Tracks.IndexOf(CurrentTrack)));
            NextTracks = new LinkedList<Track>(Tracks.Skip(Tracks.IndexOf(CurrentTrack) + 1));
            //queuePosition = queueOrder[queuePosition];
            //queueOrder = queueOrder.OrderBy(q => q).ToList();
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
            //queuePosition = queueOrder.IndexOf(Tracks.IndexOf(track));
        }

        public void Next()
        {
            if(NextTracks.Count > 0)
            {
                PreviousTracks.AddLast(CurrentTrack);
                CurrentTrack = NextTracks.First();
                NextTracks.RemoveFirst();
            }
            //if (queuePosition == Tracks.Count - 1) queuePosition = 0;
            //else queuePosition++;
        }

        public void Previous()
        {
            if(PreviousTracks.Count > 0)
            {
                NextTracks.AddFirst(CurrentTrack);
                CurrentTrack = PreviousTracks.Last();
                PreviousTracks.RemoveLast();
            }
            //if (queuePosition == 0) queuePosition = Tracks.Count - 1;
            //else queuePosition--;
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

        //    var queue = new list<queueitem>();
        //    for (var i = 0; i < tracks.count; i++)
        //    {
        //        var track = tracks[queueorder[i]];
        //        queue.add(new queueitem(track, relativequeueposition(track)));
        //    }
        //    return queue;
        }

    }
}
