using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OctoplayerBackend
{
    public class Queue
    {
        public List<Track> Tracks { get; }
        private int[] queueOrder;
        private int queuePosition;
        public Track CurrentTrack
        {
            get
            {
                return Tracks[queueOrder[queuePosition]];
            }
        }

        public Queue(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Tracks = tracks;
            queueOrder = Enumerable.Range(0, tracks.Count).ToArray();
            queuePosition = startPos;
            if (shuffle) Shuffle();
        }

        public void Shuffle()
        {
            queuePosition = 0;
            queueOrder = queueOrder.Shuffle().ToArray();
        }

        public void Unshuffle()
        {
            queuePosition = queueOrder[queuePosition];
            queueOrder = queueOrder.OrderBy(q => q).ToArray();
   
        }

        public void Next()
        {
            if (queuePosition == queueOrder.Count() - 1) queuePosition = 0;
            else queuePosition++;
        }

        public void Previous()
        {
            if (queuePosition == 0) queuePosition = queueOrder.Count() - 1;
            else queuePosition--;
        }
    }
}
