using System;
using System.Collections.Generic;
using System.Text;

namespace Octoplayer_Backend
{
    public class Library
    {
        public List<Track> Tracks { get; set; }

        public Library() 
        {
            Tracks = new List<Track>();
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
        }

        public void AddTrack(string filePath)
        {
            Tracks.Add(new Track(filePath));
        }

        public Track GetTrack(string filePath)
        {
            foreach(var track in Tracks)
            {
                if (track.FilePath == filePath) return track;
            }
            return null;
        }
    }
}
