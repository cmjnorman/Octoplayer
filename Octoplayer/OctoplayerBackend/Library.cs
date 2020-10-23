using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OctoplayerBackend
{
    public class Library
    {
        public List<Track> Tracks { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Album> Albums { get; set; }
        public List<Genre> Genres { get; set; }

        public Library(string[] files) 
        {
            this.Tracks = new List<Track>();
            this.Artists = new List<Artist>();
            this.Albums = new List<Album>();
            this.Genres = new List<Genre>();

            string[] extensions = { ".mp3", ".wav", ".flac" };
            foreach (var file in files)
            {
                if (extensions.Contains(Path.GetExtension(file))) AddTrack(file);
            }
            SaveLibrary();
        }

        public void AddTrack(string filePath)
        {
            var track = new Track(this.Tracks.Count + 1, filePath, this);
            this.Tracks.Add(track);
            this.Tracks = this.Tracks.OrderBy(a => a.Title).ToList();
        }

        public List<Artist> FindOrCreateArtists(string[] names)
        {
            var artists = new List<Artist>();
            foreach (var name in names)
            {
                var artist = this.Artists.FirstOrDefault(a => a.Name == name);
                if (artist == null)
                {
                    artist = new Artist(this.Artists.Count + 1, name);
                    this.Artists.Add(artist);
                    this.Artists = this.Artists.OrderBy(a => a.Name).ToList();
                }
                artists.Add(artist);
            }
            return artists;
        }

        public Album FindOrCreateAlbum(string title)
        {
            var album = this.Albums.FirstOrDefault(a => a.Title == title);
            if (album == null)
            {
                album = new Album(this.Albums.Count + 1, title);
                this.Albums.Add(album);
                this.Albums = this.Albums.OrderBy(a => a.Title).ToList();
            }
            return album;
        }

        public List<Genre> FindOrCreateGenres(string[] names)
        {
            var genres = new List<Genre>();
            foreach(var name in names)
            {
                var genre = this.Genres.FirstOrDefault(g => g.Name == name);
                if (genre == null)
                {
                    genre = new Genre(this.Genres.Count + 1, name);
                    this.Genres.Add(genre);
                    this.Genres = this.Genres.OrderBy(a => a.Name).ToList();
                }
                genres.Add(genre);
            }
            return genres;
        }

        public void SaveLibrary()
        {
            var document = new XDocument();

            var library = new XElement("Library");

            var trackElement = new XElement("Tracks");
            foreach (var track in this.Tracks)
            {
                var artists = new XElement("Artists");
                foreach (var artist in track.Artists)
                {
                    artists.Add(new XElement("ArtistId", artist.Id));
                }
                var genres = new XElement("Genres");
                foreach(var genre in track.Genres)
                {
                    genres.Add(new XElement("GenreId", genre.Id));
                }
                trackElement.Add(new XElement("Track",
                                    new XElement("Id", track.Id),
                                    new XElement("FilePath", track.FilePath),
                                    new XElement("Title", track.Title),
                                    artists,
                                    new XElement("AlbumId", track.Album.Id),
                                    new XElement("TrackNumber", track.TrackNumber),
                                    new XElement("TrackCount", track.TrackCount),
                                    new XElement("DiscNumber", track.DiscNumber),
                                    new XElement("DiscCount", track.DiscCount),
                                    new XElement("Year", track.Year),
                                    new XElement("Rating", track.Rating),
                                    genres,
                                    new XElement("BPM", track.BPM),
                                    new XElement("Key", track.Key)));

            }

            var artistElement = new XElement("Artists");
            foreach(var artist in this.Artists)
            {
                var tracks = new XElement("Tracks");
                foreach(var track in artist.Tracks)
                {
                    tracks.Add(new XElement("TrackId", track.Id));
                }
                artistElement.Add(new XElement("Artist",
                                    new XElement("Id", artist.Id),
                                    new XElement("Name", artist.Name),
                                    tracks));
            }

            var albumElement = new XElement("Albums");
            foreach (var album in this.Albums)
            {
                var tracks = new XElement("Tracks");
                foreach (var track in album.Tracks)
                {
                    tracks.Add(new XElement("TrackId", track.Id));
                }
                albumElement.Add(new XElement("Album",
                                    new XElement("Id", album.Id),
                                    new XElement("Title", album.Title),
                                    tracks));
            }

            var genreElement = new XElement("Genres");
            foreach (var genre in this.Genres)
            {
                var tracks = new XElement("Tracks");
                foreach (var track in genre.Tracks)
                {
                    tracks.Add(new XElement("TrackId", track.Id));
                }
                genreElement.Add(new XElement("Genre",
                                    new XElement("Id", genre.Id),
                                    new XElement("Title", genre.Name),
                                    tracks));
            }

            library.Add(trackElement, artistElement, albumElement, genreElement);
            document.Add(library);

            document.Save("library.xml");
        }
    }
}
