using System;
using System.Collections.Generic;
using System.Text;

namespace OctoplayerBackend
{
    public class QueueItem
    {
        public Track Track { get; }
        public int RelativePosition { get; }
        public string Title
        { 
            get 
            { 
                return Track.Title; 
            } 
        }

        public string Artists
        {
            get 
            {
                return String.Join("; ", Track.Artists);
            }
        }

        public QueueItem(Track track, int position)
        {
            this.Track = track;
            this.RelativePosition = position;
        }
    }
}
