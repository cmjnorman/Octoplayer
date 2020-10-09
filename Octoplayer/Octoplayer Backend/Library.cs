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
            Tracks.OrderBy(t => t.Title);
            var album = Albums.FirstOrDefault(a => a.Title == track.Album);
            if (album == null)
            {
                Albums.Add(new Album(track));
            }
            else
            {
                album.AddTrack(track);
            }
            Albums = Albums.OrderBy(t => t.Title).ToList();
        }
    }
}
