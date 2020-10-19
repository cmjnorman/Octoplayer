﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public class Artist
    {
        public string Name { get; }
        public List<Track> Tracks { get; private set; }
        public List<Album> Albums
        {
            get { return Tracks.Select(t => t.Album).Distinct().OrderBy(a => a.Title).ToList(); }
        }


        public Artist(string name)
        {
            this.Name = name;
            Tracks = new List<Track>();
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            Tracks = Tracks.OrderBy(t => t.Title).ToList();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}