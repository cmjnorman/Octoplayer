﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;
using Octoplayer_Backend;
using System.Windows.Threading;
namespace Octoplayer_Frontend
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private DispatcherTimer timer;
        private Library library;
        private Track currentTrack;
        private bool isPlaying = false;
        private bool SliderActive = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(isPlaying) Pause();
                library = new Library();
                var files = Directory.GetFiles(folderBrowser.SelectedPath, "*", SearchOption.AllDirectories);
                string[] extensions = { ".mp3", ".wav", ".flac" };
                foreach (var file in files)
                {
                    if (extensions.Contains(Path.GetExtension(file))) library.AddTrack(file);
                }
                currentTrack = library.Tracks[0];
                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                ListBoxTracks.ItemsSource = library.Tracks;
                LoadTrack();

                GridPlayer.Visibility = Visibility.Visible; 
            }
        }

        
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (player.Position.TotalSeconds >= 5) player.Position = new TimeSpan(0);
            else Previous();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void ListBoxTracks_Select(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            currentTrack = (Track) ListBoxTracks.SelectedItem;
            LoadTrack();
            Play();
        }


        private void LoadTrack()
        {
            player.Open(new Uri(currentTrack.FilePath));

            LblTrackTitle.Content = currentTrack.Title;
            LblArtists.Content = currentTrack.ArtistString;
            LblAlbum.Content = currentTrack.Album;
            ImgAlbumArt.Source = currentTrack.Artwork;

            ((System.Windows.Controls.Label)this.FindResource("TrackInfo")).Content = $"{currentTrack.TrackNumber} / {currentTrack.TrackCount}";
            ((System.Windows.Controls.Label)this.FindResource("DiscInfo")).Content = $"{currentTrack.DiscNumber} / {currentTrack.DiscCount}";
            ((System.Windows.Controls.Label)this.FindResource("Year")).Content = currentTrack.Year;
            ((System.Windows.Controls.Label)this.FindResource("Rating")).Content = currentTrack.Rating;
            ((System.Windows.Controls.Label)this.FindResource("Genres")).Content = currentTrack.GenreString;
            ((System.Windows.Controls.Label)this.FindResource("BPM")).Content = currentTrack.BPM;
            ((System.Windows.Controls.Label)this.FindResource("Key")).Content = currentTrack.Key;

            player.MediaOpened += OnTrackLoad;
            player.MediaEnded += OnTrackEnd;
            
            if (isPlaying) Play();
        }

        private void Slider_DragStarted(object sender, RoutedEventArgs e)
        {
            SliderActive = true;
            if (isPlaying) player.Pause();
        }
        
        private void Slider_DragCompleted(object sender, RoutedEventArgs e)
        {
            SliderActive = false;
            if(isPlaying) player.Play();
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if(SliderActive) player.Position = TimeSpan.FromMilliseconds(Slider.Value);
        }

        private void OnTrackLoad(object sender, EventArgs e)
        {
            Slider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void OnTrackEnd(object sender, EventArgs e)
        {
            Next();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!SliderActive)
            {
                Slider.Value = player.Position.TotalMilliseconds;
                LabelTrackTime.Content = $"{(player.Position).ToString(@"hh\:mm\:ss")} / {(player.NaturalDuration.TimeSpan).ToString(@"hh\:mm\:ss")}";
            }
        }

        private void Previous()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            if (currentTrackIndex == 0) currentTrack = library.Tracks[library.Tracks.Count - 1];
            else
            {
                timer.Stop();
                currentTrack = library.Tracks[currentTrackIndex - 1];
            }
            LoadTrack();
        }

        private void Next()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            timer.Stop();
            if (currentTrackIndex + 1 == library.Tracks.Count) currentTrack = library.Tracks[0];
            else currentTrack = library.Tracks[currentTrackIndex + 1];
            LoadTrack();
            
        }

        private void Play()
        {
            player.Play();
            BtnPlayPause.Content = FindResource("Pause");
            isPlaying = true;
        }

        private void Pause()
        {
            player.Pause();
            BtnPlayPause.Content = FindResource("Play");
            isPlaying = false;
        }
    }
}
