using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public class Genre
    {
        public int Id { get; }
        public string Name { get; set; }
        public List<Track> Tracks { get; set; }

        public Genre(int id, string name)
        {
            this.Id = id;
            this.Name = name;
            Tracks = new List<Track>();
        }

        public Genre(int id, string name, List<Track> tracks)
        {
            this.Id = id;
            this.Name = name;
            this.Tracks = tracks;
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
