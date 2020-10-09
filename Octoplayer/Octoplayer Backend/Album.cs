using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Octoplayer_Backend
{
    public class Album
    {
        public string Title { get; set; }
        public List<Track> Tracks { get; set; }
        public List<string> Artists { get; set; }
        public BitmapImage Artwork
        { 
            get
            {
                return Tracks[0].Artwork;
            }
        }

        public Album(Track track)
        {
            this.Title = track.Album;
            Tracks = new List<Track>() { track };
            Artists = new List<string>();
            Artists.AddRange(track.Artists);
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            Tracks = Tracks.OrderBy(t => t.TrackNumber).ToList();
            Artists.AddRange(track.Artists);
            Artists.Sort();
        }
    }
}
