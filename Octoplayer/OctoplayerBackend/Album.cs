using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public class Album : IBrowsable
    {
        public string Title { get; }
        public List<Track> Tracks { get; private set; }
        public List<Artist> Artists
        {
            get { return Tracks.SelectMany(t => t.Artists).Distinct().OrderBy(a => a.Name).ToList(); } 
        }
        public BitmapImage Artwork
        { 
            get { return Tracks[0].Artwork; }
        }
        string IBrowsable.Heading
        {
            get { return Title; }
        }
        string IBrowsable.SubHeading1
        {
            get { return String.Join("; ", Artists); }
        }
        string IBrowsable.SubHeading2
        {
            get { return ""; }
        }
        BitmapImage IBrowsable.Image
        {
            get { return Artwork; }
        }

        public Album(string title)
        {
            this.Title = title;
            Tracks = new List<Track>();
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            Tracks = Tracks.OrderBy(t => t.TrackNumber).ToList();
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
