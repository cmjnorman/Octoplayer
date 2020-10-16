using System.Text;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;

namespace Octoplayer_Backend
{
    public class Track
    {
        public string FilePath { get; private set; }
        public string Title { get; set; }
        public List<Artist> Artists { get; set; }
        public string ArtistString
        {
            get
            {
                if (Artists.Count == 0) return "";
                var artists = new StringBuilder();
                artists.Append(Artists[0]);
                for (var i = 1; i < Artists.Count; i++)
                {
                    artists.Append($"; {Artists[i]}");
                }
                return artists.ToString();
            }
        }
        public Album Album { get; set; }
        public uint TrackNumber { get; set; }
        public uint TrackCount { get; set; }
        public uint DiscNumber { get; set; }
        public uint DiscCount { get; set; }
        public uint Year { get; set; }
        public uint Rating { get; set; }
        public List<Genre> Genres { get; set; }
        public string GenreString
        {
            get
            {
                if (Genres.Count == 0) return "";
                var genres = new StringBuilder();
                genres.Append(Genres[0]);
                for (var i = 1; i < Genres.Count; i++)
                {
                    genres.Append($"; {Genres[i]}");
                }
                return genres.ToString();
            }
        }
        public uint BPM { get; set; }
        public string Key { get; set; }
        public BitmapImage Artwork { get; set; }

        public Track(string filepath, Library lib)
        {
            this.FilePath = filepath;
            var track = TagLib.File.Create(filepath);

            this.Title = track.Tag.Title;
            this.TrackNumber = track.Tag.Track;
            this.TrackCount = track.Tag.TrackCount;
            this.DiscNumber = track.Tag.Disc;
            this.DiscCount = track.Tag.DiscCount;
            this.Year = track.Tag.Year;
            this.BPM = track.Tag.BeatsPerMinute;
            this.Key = track.Tag.InitialKey;
            if (track.Tag.Pictures.Length > 0)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                bitmap.EndInit();
                this.Artwork = bitmap;
            }

            this.Artists = lib.FindOrCreateArtists(track.Tag.Performers[0].Split("; "));
            this.Artists.ForEach(a => a.AddTrack(this));

            this.Album = lib.FindOrCreateAlbum(track.Tag.Album);
            this.Album.AddTrack(this);

            this.Genres = lib.FindOrCreateGenres(track.Tag.Genres[0].Split("; "));
            this.Genres.ForEach(g => g.AddTrack(this));
        }
    }
}
