using System;
using System.Collections.Generic;
using System.Text;

namespace OctoplayerBackend
{
    public class Queue
    {
        
        public List<Track> PlayingQueue { get; }
        private int queuePosition;
        public Track CurrentTrack
        {
            get
            {
                return PlayingQueue[queuePosition];
            }
        }

        public Queue(List<Track> tracks)
        {
            this.PlayingQueue = tracks;
            queuePosition = 0;
        }

        public void Next()
        {
            if (queuePosition == PlayingQueue.Count - 1) queuePosition = 0;
            else queuePosition++;
        }

        public void Previous()
        {
            if (queuePosition == 0) queuePosition = PlayingQueue.Count - 1;
            else queuePosition--;
        }
    }
}
