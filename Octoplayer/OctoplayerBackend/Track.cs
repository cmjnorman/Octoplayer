using System.Text;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;

namespace OctoplayerBackend
{
    public class Track
    {
        public string FilePath { get; private set; }
        public string Title { get; set; }
        public List<Artist> Artists { get; set; }
        public Album Album { get; set; }
        public uint TrackNumber { get; set; }
        public uint TrackCount { get; set; }
        public uint DiscNumber { get; set; }
        public uint DiscCount { get; set; }
        public List<Artist> Remixers { get; set; }
        public uint Year { get; set; }
        public uint Rating { get; set; }
        public List<Genre> Genres { get; set; }
        public uint BPM { get; set; }
        public string Key { get; set; }
        public BitmapImage Artwork { get; set; }
        public uint PlayCount { get; set; }
        public DateTime LastPlayed { get; set; }


        public Track(string filePath, Library lib)
        {
            this.FilePath = filePath;
            var track = TagLib.File.Create(filePath);

            this.Title = track.Tag.Title;
            this.TrackNumber = track.Tag.Track;
            this.TrackCount = track.Tag.TrackCount;
            this.DiscNumber = track.Tag.Disc;
            this.DiscCount = track.Tag.DiscCount;
            this.Year = track.Tag.Year;
            this.Rating = 50;
            this.BPM = track.Tag.BeatsPerMinute;
            this.Key = track.Tag.InitialKey;
            this.PlayCount = 0;
            if (track.Tag.Pictures.Length > 0)
            { 
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                    bitmap.EndInit();
                    this.Artwork = bitmap;
                }
                catch (Exception) { }
            }

            this.Artists = new List<Artist>();
            foreach (var artist in track.Tag.Performers)
            {
                this.Artists.AddRange(lib.FindOrCreateArtists(artist.Split("; ")));
            }
            this.Artists.ForEach(a => a.AddTrack(this));

            this.Remixers = new List<Artist>();
            var remixers = track.Tag.RemixedBy;
            if (remixers != null)
            {
                this.Remixers.AddRange(lib.FindOrCreateArtists(remixers.Split("; ")));
            }
            this.Remixers.ForEach(a => a.AddRemix(this));

            this.Album = lib.FindOrCreateAlbum(track.Tag.Album);
            this.Album.AddTrack(this);

            this.Genres = new List<Genre>();
            foreach (var genre in track.Tag.Genres)
            {
                this.Genres.AddRange(lib.FindOrCreateGenres(genre.Split("; ")));
            }
            this.Genres.ForEach(g => g.AddTrack(this));
        }

        public Track(string filePath, string title, uint rating, uint playCount, DateTime lastPlayed, Library lib)
        {
            this.FilePath = filePath;
            var track = TagLib.File.Create(filePath);
            this.Title = title;
            this.Rating = rating;
            this.PlayCount = playCount;
            this.LastPlayed = lastPlayed;
            this.TrackNumber = track.Tag.Track;
            this.TrackCount = track.Tag.TrackCount;
            this.DiscNumber = track.Tag.Disc;
            this.DiscCount = track.Tag.DiscCount;
            this.Year = track.Tag.Year;
            this.BPM = track.Tag.BeatsPerMinute;
            this.Key = track.Tag.InitialKey;
            if (track.Tag.Pictures.Length > 0)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                    bitmap.EndInit();
                    this.Artwork = bitmap;
                }
                catch (Exception) { }
            }

            this.Artists = new List<Artist>();
            foreach (var artist in track.Tag.Performers)
            {
                this.Artists.AddRange(lib.FindOrCreateArtists(artist.Split("; ")));
            }
            this.Artists.ForEach(a => a.AddTrack(this));

            this.Remixers = new List<Artist>();
            var remixers = track.Tag.RemixedBy;
            if (remixers != null)
            {
                this.Remixers.AddRange(lib.FindOrCreateArtists(remixers.Split("; ")));
            }
            this.Remixers.ForEach(a => a.AddRemix(this));

            this.Album = lib.FindOrCreateAlbum(track.Tag.Album);
            this.Album.AddTrack(this);

            this.Genres = new List<Genre>();
            foreach(var genre in track.Tag.Genres)
            {
                this.Genres.AddRange(lib.FindOrCreateGenres(genre.Trim().Split("; "))); 
            }
            this.Genres.ForEach(g => g.AddTrack(this));
        }
    }
}
