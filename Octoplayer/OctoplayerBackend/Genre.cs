using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public class Genre : IBrowsable
    {
        public string Name { get; set; }
        public List<Track> Tracks { get; set; }
        string IBrowsable.Heading
        {
            get { return Name; }
        }
        string IBrowsable.SubHeading1
        {
            get { return ""; }
        }
        string IBrowsable.SubHeading2
        {
            get { return ""; }
        }
        BitmapImage IBrowsable.Image
        {
            get { return Tracks[0].Artwork; }
        }

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
