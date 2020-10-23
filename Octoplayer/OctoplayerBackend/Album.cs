using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public class Album
    {
        public int Id { get; }
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

        public Album(int id, string title)
        {
            this.Id = id;
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
