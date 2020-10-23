using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;
using OctoplayerBackend;
using System.Windows.Threading;
using System.Collections.Generic;

namespace OctoplayerFrontend
{
    public partial class MainWindow : Window
    {
        private Player player = new Player();
        private DispatcherTimer timelineClock;
        private Library library;
        private bool trackSliderBeingDragged = false;
        private bool ShuffleEnabled = false;


        public MainWindow()
        {
            InitializeComponent();
            player.TrackLoaded += OnTrackLoad;
            player.MediaPlaying += () => BtnPlayPause.Content = FindResource("Pause");
            player.MediaPaused += () => BtnPlayPause.Content = FindResource("Play");
            GridPlayer.Visibility = Visibility.Hidden;
            LibraryBrowser.Visibility = Visibility.Hidden;
            BtnBack.Visibility = Visibility.Collapsed;
            BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
            library = new Library();
            if (library.Tracks.Count > 0)
            {
                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                ListBoxTracks.ItemsSource = library.Tracks;
                ListBoxAlbums.ItemsSource = library.Albums;
                ListBoxArtists.ItemsSource = library.Artists;
                ListBoxGenres.ItemsSource = library.Genres;
                LibraryBrowser.Visibility = Visibility.Visible;
                ToggleListBox(ListBoxTracks);
                BtnViewTracks.IsEnabled = false;
            }
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(player.IsPlaying) player.Pause();
                UnloadTrack();

                library = new Library(Directory.GetFiles(folderBrowser.SelectedPath, "*", SearchOption.AllDirectories));

                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                ListBoxTracks.ItemsSource = library.Tracks;
                ListBoxAlbums.ItemsSource = library.Albums;
                ListBoxArtists.ItemsSource = library.Artists;
                ListBoxGenres.ItemsSource = library.Genres;
                LibraryBrowser.Visibility = Visibility.Visible;
                ToggleListBox(ListBoxTracks);
                BtnViewTracks.IsEnabled = false;
            }
        }

        
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (player.IsPlaying)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            player.Previous();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            player.Next();
        }

        private void BtnViewTracks_Click(object sender, RoutedEventArgs e)
        {
            BtnViewTracks.IsEnabled = false;
            BtnViewAlbums.IsEnabled = BtnViewArtists.IsEnabled = BtnViewGenres.IsEnabled = true;
            ToggleListBox(ListBoxTracks);
            BtnBack.Visibility = Visibility.Collapsed;
            BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
        }

        private void BtnViewAlbums_Click(object sender, RoutedEventArgs e)
        {
            BtnViewAlbums.IsEnabled = false;
            BtnViewTracks.IsEnabled = BtnViewArtists.IsEnabled = BtnViewGenres.IsEnabled = true;
            ToggleListBox(ListBoxAlbums);
            BtnBack.Visibility = Visibility.Collapsed;
            BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
        }

        private void BtnViewArtists_Click(object sender, RoutedEventArgs e)
        {
            BtnViewArtists.IsEnabled = false;
            BtnViewTracks.IsEnabled = BtnViewAlbums.IsEnabled = BtnViewGenres.IsEnabled = true;
            ToggleListBox(ListBoxArtists);
            BtnBack.Visibility = Visibility.Collapsed;
            BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
        }

        private void BtnViewGenres_Click(object sender, RoutedEventArgs e)
        {
            BtnViewGenres.IsEnabled = false;
            BtnViewTracks.IsEnabled = BtnViewArtists.IsEnabled = BtnViewAlbums.IsEnabled = true;
            ToggleListBox(ListBoxGenres);
            BtnBack.Visibility = Visibility.Collapsed;
            BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
        }

        private void ToggleListBox(System.Windows.Controls.ListBox box)
        {
            foreach (var listBox in ListBoxGrid.Children.OfType<System.Windows.Controls.ListBox>())
            {
                listBox.Visibility = (listBox == box ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        private void ListBoxTracks_Select(object sender, RoutedEventArgs e)
        {
            if(ListBoxTracks.SelectedItem != null)
            {
                var tracks = ListBoxTracks.Items.OfType<Track>();
                var index = ListBoxTracks.SelectedIndex;
                var queue = tracks.Skip(index).Concat(tracks.Take(index));
                if (ShuffleEnabled) player.SetQueue(queue.Take(1).Concat(queue.Skip(1).Shuffle()).ToList());
                else player.SetQueue(queue.ToList());
            }
        }

        private void ListBoxAlbums_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxAlbums.SelectedItem != null)
            {
                ListBoxAlbumTracks.ItemsSource = ((Album)ListBoxAlbums.SelectedItem).Tracks;
                ToggleListBox(ListBoxAlbumTracks);
                BtnBack.Visibility = Visibility.Visible;
            }
        }

        private void ListBoxArtists_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxArtists.SelectedItem != null)
            {
                ListBoxTracksSubmenu.ItemsSource = ((Artist)ListBoxArtists.SelectedItem).Tracks;
                ListBoxAlbumsSubmenu.ItemsSource = ((Artist)ListBoxArtists.SelectedItem).Albums;
                ToggleListBox(ListBoxTracksSubmenu);
                BtnBack.Visibility = Visibility.Visible;
                BtnSwapTrackAlbum.Content = "Show Artist Albums";
                BtnSwapTrackAlbum.Visibility = Visibility.Visible;
            }
        }

        private void ListBoxGenres_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxGenres.SelectedItem != null)
            {
                ListBoxTracksSubmenu.ItemsSource = ((Genre)ListBoxGenres.SelectedItem).Tracks;
                ToggleListBox(ListBoxTracksSubmenu);
                BtnBack.Visibility = Visibility.Visible;
            }
        }

        private void ListBoxAlbumTracks_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxAlbumTracks.SelectedItem != null)
            {
                var tracks = ListBoxAlbumTracks.Items.OfType<Track>();
                var index = ListBoxAlbumTracks.SelectedIndex;
                var queue = tracks.Skip(index).Concat(tracks.Take(index));
                if (ShuffleEnabled) player.SetQueue(queue.Take(1).Concat(queue.Skip(1).Shuffle()).ToList());
                else player.SetQueue(queue.ToList());
            }
        }

        private void ListBoxTracksSubmenu_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxTracksSubmenu.SelectedItem != null)
            {
                var tracks = ListBoxTracksSubmenu.Items.OfType<Track>();
                var index = ListBoxTracksSubmenu.SelectedIndex;
                var queue = tracks.Skip(index).Concat(tracks.Take(index));
                if (ShuffleEnabled) player.SetQueue(queue.Take(1).Concat(queue.Skip(1).Shuffle()).ToList());
                else player.SetQueue(queue.ToList());
            }
        }

        private void ListBoxAlbumsSubmenu_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxAlbumsSubmenu.SelectedItem != null)
            {
                ListBoxAlbumTracks.ItemsSource = ((Album)ListBoxAlbumsSubmenu.SelectedItem).Tracks;
                ToggleListBox(ListBoxAlbumTracks);
                BtnBack.Visibility = Visibility.Visible;
            }
        }

        private void OnTrackLoad()
        {
            if (GridPlayer.Visibility == Visibility.Hidden) GridPlayer.Visibility = Visibility.Visible;

            var track = player.Queue.CurrentTrack;
            LblTrackTitle.Content = track.Title;
            LblArtists.Content = String.Join("; ", track.Artists);
            LblAlbum.Content = track.Album;
            ImgAlbumArt.Source = track.Artwork;

            ((System.Windows.Controls.Label)this.FindResource("TrackInfo")).Content = $"{track.TrackNumber} / {track.TrackCount}";
            ((System.Windows.Controls.Label)this.FindResource("DiscInfo")).Content = $"{track.DiscNumber} / {track.DiscCount}";
            ((System.Windows.Controls.Label)this.FindResource("Year")).Content = track.Year;
            ((System.Windows.Controls.Label)this.FindResource("Rating")).Content = track.Rating;
            ((System.Windows.Controls.Label)this.FindResource("Genres")).Content = String.Join("; ", track.Genres);
            ((System.Windows.Controls.Label)this.FindResource("BPM")).Content = track.BPM;
            ((System.Windows.Controls.Label)this.FindResource("Key")).Content = track.Key;

            BtnNext.IsEnabled = BtnPlayPause.IsEnabled = BtnPrevious.IsEnabled = TrackSlider.IsEnabled = true;
            TrackSlider.Maximum = player.CurrentTrackLength;
            timelineClock = new DispatcherTimer();
            timelineClock.Interval = TimeSpan.FromMilliseconds(1);
            timelineClock.Tick += timelineClock_Tick;
            timelineClock.Start();
        }

        public void UnloadTrack()
        {
            if (player.IsPlaying) player.Pause();
            if(timelineClock != null) timelineClock.Stop();
            GridPlayer.Visibility = Visibility.Hidden;
            BtnNext.IsEnabled = BtnPlayPause.IsEnabled = BtnPrevious.IsEnabled = TrackSlider.IsEnabled = false;
            TrackSlider.Value = 0;
            LabelTrackTime.Content = "";
        }

        private void TrackSlider_DragStarted(object sender, RoutedEventArgs e)
        {
            trackSliderBeingDragged = true;
            if (player.IsPlaying) player.Suspend();
        }
        
        private void TrackSlider_DragCompleted(object sender, RoutedEventArgs e)
        {
            trackSliderBeingDragged = false;
            if (player.IsPlaying) player.Unsuspend();
        }

        private void TrackSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            player.CurrentTrackPosition = TrackSlider.Value;
        }
 
        private void timelineClock_Tick(object sender, EventArgs e)
        {
            if (!trackSliderBeingDragged)
            {
                TrackSlider.Value = player.CurrentTrackPosition;
                LabelTrackTime.Content = $"{TimeSpan.FromMilliseconds(player.CurrentTrackPosition).ToString(@"hh\:mm\:ss")} / {TimeSpan.FromMilliseconds(player.CurrentTrackLength).ToString(@"hh\:mm\:ss")}";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (!BtnViewAlbums.IsEnabled)
            {
                ToggleListBox(ListBoxAlbums);
                BtnBack.Visibility = Visibility.Collapsed;
            }
            else if (!BtnViewArtists.IsEnabled)
            {
                if (ListBoxAlbumTracks.Visibility == Visibility.Visible) ToggleListBox(ListBoxAlbumsSubmenu);
                else
                {
                    ToggleListBox(ListBoxArtists);
                    BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
                    BtnBack.Visibility = Visibility.Collapsed;
                }
            }
            else if (!BtnViewGenres.IsEnabled)
            {
                ToggleListBox(ListBoxGenres);
                BtnBack.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSwapTrackAlbum_Click(object sender, RoutedEventArgs e)
        {
            if((string)BtnSwapTrackAlbum.Content == "Show Artist Albums")
            {
                ToggleListBox(ListBoxAlbumsSubmenu);
                BtnSwapTrackAlbum.Content = "Show Artist Tracks";
            }
            else
            {
                ToggleListBox(ListBoxTracksSubmenu);
                BtnSwapTrackAlbum.Content = "Show Artist Albums";
            }
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            ShuffleEnabled = !ShuffleEnabled;
        }
    }
}
