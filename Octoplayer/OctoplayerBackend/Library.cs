﻿using System;
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
        public List<string> libraryFolders { get; }

        public Library()
        {
            this.Tracks = new List<Track>();
            this.Artists = new List<Artist>();
            this.Albums = new List<Album>();
            this.Genres = new List<Genre>();
            this.libraryFolders = new List<string>();
        }

        public Library(List<string> files, List<string> libraryFolders) 
        {
            this.Tracks = new List<Track>();
            this.Artists = new List<Artist>();
            this.Albums = new List<Album>();
            this.Genres = new List<Genre>();
            this.libraryFolders = libraryFolders;
            
            foreach (var file in files)
            {
                AddTrack(file);
            }
        }

        public void AddTrack(string filePath)
        {
            var track = new Track(filePath, this);
            this.Tracks.Add(track);
            this.Tracks = this.Tracks.OrderBy(a => a.Title).ToList();
        }

        public void UpdateTrackRatings(Track track, double change)
        {
            track.ChangeRating(change);
            var fraction = -change  / (Tracks.Count - 1);
            foreach (var otherTrack in Tracks.Where(t => t != track))
            {
                otherTrack.ChangeRating(fraction);
            }
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
                album = new Album(title);
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
                    genre = new Genre(name);
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

            var folders = new XElement("LibraryFolders");
            foreach(var folder in this.libraryFolders)
            {
                folders.Add(new XElement("FolderPath", folder));
            }

            var tracks = new XElement("Tracks");
            foreach (var track in this.Tracks)
            {
                tracks.Add(new XElement("Track",
                                    new XElement("FilePath", track.FilePath),
                                    new XElement("Title", track.Title),
                                    new XElement("Rating", track.Rating),
                                    new XElement("PlayCount", track.PlayCount),
                                    new XElement("LastPlayed", track.LastPlayed)));
            }

            var library = new XElement("Library", folders, tracks);
            document.Add(library);
            document.Save("library.xml");
        }

        public bool LoadLibrary()
        {
            try
            {
                var folders = XDocument.Load("library.xml").Element("Library").Element("LibraryFolders").Elements("FolderPath");
                foreach (var folder in folders)
                {
                    this.libraryFolders.Add(folder.Value);
                }

                var tracks = XDocument.Load("library.xml").Element("Library").Element("Tracks").Elements("Track");
                foreach (var track in tracks)
                {
                    this.Tracks.Add(new Track(track.Element("FilePath").Value,
                                        track.Element("Title").Value,
                                        Double.Parse(track.Element("Rating").Value),
                                        UInt32.Parse(track.Element("PlayCount").Value),
                                        DateTime.Parse(track.Element("LastPlayed").Value),
                                        this));
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }
    }
}
