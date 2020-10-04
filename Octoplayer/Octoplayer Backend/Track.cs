using System;
using System.Collections.Generic;
using System.Text;
using TagLib;

namespace Octoplayer_Backend
{
    public class Track
    {
        public string FilePath { get; private set; }
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public string Album { get; set; }
        public uint TrackNumber { get; set; }
        public uint TrackCount { get; set; }
        public uint DiscNumber { get; set; }
        public uint DiscCount { get; set; }
        public uint Year { get; set; }
        public uint Rating { get; set; }
        public string[] Genres { get; set; }
        public uint BPM { get; set; }
        public string Key { get; set; }
        public IPicture Artwork { get; set; }

        public Track(string filepath)
        {
            this.FilePath = filepath;
            var track = TagLib.File.Create(filepath);

            this.Title = track.Tag.Title;
            this.Artists = track.Tag.Performers;
            this.Album = track.Tag.Album;
            this.TrackNumber = track.Tag.Track;
            this.TrackCount = track.Tag.TrackCount;
            this.DiscNumber = track.Tag.Disc;
            this.DiscCount = track.Tag.DiscCount;
            this.Year = track.Tag.Year;
            this.Genres = track.Tag.Genres;
            this.BPM = track.Tag.BeatsPerMinute;
            this.Key = track.Tag.InitialKey;
            this.Artwork = track.Tag.Pictures[0];
        }

        public string GetArtistString()
        {
            var artists = new StringBuilder();
            artists.Append(Artists[0]);
            for(var i = 1; i < Artists.Length; i++)
            {
                artists.Append($"; {Artists[i]}");
            }
            return artists.ToString();
        }

        public string GetGenreString()
        {
            var genres = new StringBuilder();
            genres.Append(Genres[0]);
            for (var i = 1; i < Genres.Length; i++)
            {
                genres.Append($"; {Genres[i]}");
            }
            return genres.ToString();
        }
    }
}
