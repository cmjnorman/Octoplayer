using System;
using System.Collections.Generic;
using System.Text;

namespace Octoplayer_Backend
{
    public class Album
    {
        public string Title { get; set; }
        public List<Track> Tracks { get; set; }

        public Album(Track track)
        {
            this.Title = track.Title;
            Tracks = new List<Track>() { track };
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
        }
    }
}
