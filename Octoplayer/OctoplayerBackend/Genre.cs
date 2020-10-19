using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OctoplayerBackend
{
    public class Genre
    {
        public string Name { get; set; }
        public List<Track> Tracks { get; set; }


        public Genre(string name)
        {
            this.Name = name;
            Tracks = new List<Track>();
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            Tracks = Tracks.OrderBy(t => t.Title).ToList();
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
