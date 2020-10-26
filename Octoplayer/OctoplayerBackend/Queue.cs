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
        private int currentIndex;
        public Track CurrentTrack
        {
            get
            {
                return Tracks[currentIndex];
            }
        }

        public Queue(List<Track> tracks, int startPos, bool shuffle)
        {
            this.Tracks = tracks;
            queueOrder = Enumerable.Range(0, tracks.Count).ToArray();
            currentIndex = startPos;
            if (shuffle) Shuffle();
        }

        public void Shuffle()
        {
            queueOrder = queueOrder.Shuffle().ToArray();
        }

        public void Unshuffle()
        {
            queueOrder = queueOrder.OrderBy(q => q).ToArray();
        }

        public void Next()
        {
            if (currentIndex == queueOrder.Last()) currentIndex = queueOrder.First();
            else currentIndex = queueOrder[Array.IndexOf(queueOrder, currentIndex) + 1];
        }

        public void Previous()
        {
            if (currentIndex == queueOrder.First()) currentIndex = queueOrder.Last();
            else currentIndex = queueOrder[Array.IndexOf(queueOrder, currentIndex) - 1];
        }
    }
}
