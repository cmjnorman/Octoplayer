using System;
using System.Collections.Generic;
using System.Linq;

namespace Octoplayer_Backend
{
    public class Library
    {
        public List<Track> Tracks { get; set; }
        public List<Album> Albums { get; set; }
 
        public Library() 
        {
            Tracks = new List<Track>();
            Albums = new List<Album>();
        }

        public void AddTrack(string filePath)
        {
            var track = new Track(filePath);
            Tracks.Add(track);
            var album = Albums.FirstOrDefault(a => a.Title == track.Album);
            if (album == null)
            {
                Albums.Add(new Album(track));
            }
            else
            {
                album.AddTrack(track);
            }
        }

        public Track GetTrack(string filePath)
        {
            foreach(var track in Tracks)
            {
                if (track.FilePath == filePath) return track;
            }
            return null;
        }

        public void sortByTrackTitle()
        {
            Tracks = Tracks.OrderBy(t => t.Title).ToList();
        }
    }
}
