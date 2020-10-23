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
        public uint Year { get; set; }
        public uint Rating { get; set; }
        public List<Genre> Genres { get; set; }
        public uint BPM { get; set; }
        public string Key { get; set; }
        public BitmapImage Artwork { get; set; }


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
                catch (Exception e) { }
            }

            if(track.Tag.Performers.Length > 0)
            {
                this.Artists = lib.FindOrCreateArtists(track.Tag.Performers[0].Split("; "));
                this.Artists.ForEach(a => a.AddTrack(this));
            }
            
            this.Album = lib.FindOrCreateAlbum(track.Tag.Album);
            this.Album.AddTrack(this);

            if (track.Tag.Genres.Length > 0)
            {
                this.Genres = lib.FindOrCreateGenres(track.Tag.Genres[0].Split("; "));
                this.Genres.ForEach(g => g.AddTrack(this));
            }
        }

        public Track(string filePath, string title, uint rating, Library lib)
        {
            this.FilePath = filePath;
            var track = TagLib.File.Create(filePath);
            this.Title = title;
            this.Rating = rating;
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

            if (track.Tag.Performers.Length > 0)
            {
                this.Artists = lib.FindOrCreateArtists(track.Tag.Performers[0].Split("; "));
                this.Artists.ForEach(a => a.AddTrack(this));
            }

            this.Album = lib.FindOrCreateAlbum(track.Tag.Album);
            this.Album.AddTrack(this);

            if (track.Tag.Genres.Length > 0)
            {
                this.Genres = lib.FindOrCreateGenres(track.Tag.Genres[0].Split("; "));
                this.Genres.ForEach(g => g.AddTrack(this));
            }
        }
    }
}
