using System;
using System.Collections.Generic;
using System.Text;

namespace OctoplayerBackend
{
    public class QueueItem
    {
        public Track Track { get; }
        public int RelativePosition { get; }
        public string Title => Track.Title;
        public string Artists => String.Join("; ", Track.Artists);

        public QueueItem(Track track, int position)
        {
            this.Track = track;
            this.RelativePosition = position;
        }
    }
}
