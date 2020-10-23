using System.Text;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OctoplayerBackend
{
    public class Track
    {
        public int Id { get; }
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


        public Track(int id, string filePath, Library lib)
        {
            this.Id = id;
            this.FilePath = filePath;
            var track = TagLib.File.Create(filePath);

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

        public Track(int id, string filePath, string title, List<Artist> artists, Album album, uint trackNumber, uint trackCount, uint discNumber, uint discCount, uint year, uint rating, List<Genre> genres, uint bpm, string key)
        {
            this.Id = id;
            this.FilePath = filePath;
            this.Title = title;
            this.Artists = artists;
            this.Album = album;
            this.TrackNumber = trackNumber;
            this.TrackCount = trackCount;
            this.DiscNumber = discNumber;
            this.DiscCount = discCount;
            this.Year = year;
            this.Rating = rating;
            this.Genres = genres;
            this.BPM = bpm;
            this.Key = key;
            var track = TagLib.File.Create(filePath);
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
                catch (Exception) 
                {
                    this.Artwork = Bitmap.
                }
            }
        }
    }
}
