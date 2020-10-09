using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octoplayer_Backend
{
    public class Album
    {
        public string Title { get; set; }
        public List<Track> Tracks { get; set; }
        public List<string> Artists { get; set; }

        public Album(Track track)
        {
            this.Title = track.Title;
            Tracks = new List<Track>() { track };
            Tracks = Tracks.OrderBy(t => t.TrackNumber).ToList();
            Artists = new List<string>();
            Artists.AddRange(track.Artists);
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
        }
    }
}
