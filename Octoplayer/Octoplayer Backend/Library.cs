using System;
using System.Collections.Generic;
using System.Linq;

namespace Octoplayer_Backend
{
    public class Library
    {
        public List<Track> Tracks { get; set; }
        public List<Album> Albums { get; set; }
        public List<Artist> Artists { get; set; }
 
        public Library() 
        {
            this.Tracks = new List<Track>();
            this.Albums = new List<Album>();
            this.Artists = new List<Artist>();
        }

        public void AddTrack(string filePath)
        {
            var track = new Track(filePath, this);
            this.Tracks.Add(track);
        }

        public Album FindOrCreateAlbum(string title)
        {
            var album = this.Albums.FirstOrDefault(a => a.Title == title);
            if (album == null)
            {
                album = new Album(title);
                this.Albums.Add(album);
            }
            return album;
        }

        public List<Artist> FindOrCreateArtists(string[] names)
        {
            var artists = new List<Artist>();
            foreach (var name in names)
            {
                var artist = this.Artists.FirstOrDefault(a => a.Name == name);
                if (artist == null)
                {
                    artist = new Artist(name);
                    this.Artists.Add(artist);
                }
                artists.Add(artist);
            }
            return artists;
        }
    }
}
