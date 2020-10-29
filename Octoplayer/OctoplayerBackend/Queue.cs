using System;
using System.Linq;

namespace OctoplayerBackend
{
    public class Queue
    {
        public Track[] Tracks { get; }
        private int[] queueOrder;
        private int queuePosition;
        public Track CurrentTrack
        {
            get
            {
                return Tracks[queueOrder[queuePosition]];
            }
        }

        public Queue(Track[] tracks, int startPos, bool shuffle)
        {
            this.Tracks = tracks;
            this.queueOrder = Enumerable.Range(0, tracks.Length).ToArray();
            this.queuePosition = startPos;
            if (shuffle) Shuffle();
        }

        public void Shuffle()
        {
            queueOrder = queueOrder.Shuffle().ToArray();
            var index = Array.IndexOf(queueOrder, queuePosition);
            queueOrder = queueOrder.Skip(index).Concat(queueOrder.Take(index)).ToArray();
            queuePosition = 0;
        }

        public void Unshuffle()
        {
            queuePosition = queueOrder[queuePosition];
            queueOrder = queueOrder.OrderBy(q => q).ToArray();
        }

        public void Next()
        {
            if (queuePosition == Tracks.Length - 1) queuePosition = 0;
            else queuePosition++;
        }

        public void Previous()
        {
            if (queuePosition == 0) queuePosition = Tracks.Length - 1;
            else queuePosition--;
        }

        private int RelativeQueuePosition(Track track)
        {
            var queuedTrack = Tracks.FirstOrDefault(t => t == track);
            if (queuedTrack == null) throw new Exception("Track not found in queue");
            var positionInQueue = Array.IndexOf(queueOrder, Array.IndexOf(Tracks, queuedTrack));
            return positionInQueue - this.queuePosition;
        }

        public QueueItem[] GetQueueItems()
        {
            var queue = new QueueItem[Tracks.Length];
            for(var i = 0; i < Tracks.Length; i++)
            {
                var track = Tracks[queueOrder[i]];
                queue[i] = new QueueItem(track, RelativeQueuePosition(track));
            }
            return queue;
        }

    }
}
