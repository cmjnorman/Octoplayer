using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;
using OctoplayerBackend;
using System.Windows.Threading;

namespace OctoplayerFrontend
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private DispatcherTimer timer;
        private Library library;
        private Track currentTrack;
        private bool isPlaying = false;
        private bool trackSliderBeingDragged = false;


        public MainWindow()
        {
            InitializeComponent();
            player.MediaOpened += OnTrackLoad;
            player.MediaEnded += OnTrackEnd;
            GridPlayer.Visibility = Visibility.Hidden;
            LibraryBrowser.Visibility = Visibility.Hidden;
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(isPlaying) Pause();
                UnloadTrack();
                library = new Library();
                var files = Directory.GetFiles(folderBrowser.SelectedPath, "*", SearchOption.AllDirectories);
                string[] extensions = { ".mp3", ".wav", ".flac" };
                foreach (var file in files)
                {
                    if (extensions.Contains(Path.GetExtension(file))) library.AddTrack(file);
                }
                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                ListBoxTracks.ItemsSource = library.Tracks;
                ListBoxAlbums.ItemsSource = library.Albums;
                ListBoxArtists.ItemsSource = library.Artists;
                ListBoxGenres.ItemsSource = library.Genres;
                LibraryBrowser.Visibility = Visibility.Visible;
                BtnBack.Visibility = Visibility.Collapsed;
                BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
                ToggleListBox(ListBoxTracks);
                BtnViewTracks.IsEnabled = false;
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
                LoadTrack((Track)ListBoxTracks.SelectedItem);
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
                LoadTrack((Track)ListBoxAlbumTracks.SelectedItem);
            }
        }

        private void ListBoxTracksSubmenu_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxTracksSubmenu.SelectedItem != null)
            {
                LoadTrack((Track)ListBoxTracksSubmenu.SelectedItem);
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


        private void LoadTrack(Track track)
        {
            currentTrack = track;
            player.Open(new Uri(currentTrack.FilePath));

            if (GridPlayer.Visibility == Visibility.Collapsed) GridPlayer.Visibility = Visibility.Visible;

            LblTrackTitle.Content = currentTrack.Title;
            LblArtists.Content = String.Join("; ", currentTrack.Artists);
            LblAlbum.Content = currentTrack.Album;
            ImgAlbumArt.Source = currentTrack.Artwork;

            ((System.Windows.Controls.Label)this.FindResource("TrackInfo")).Content = $"{currentTrack.TrackNumber} / {currentTrack.TrackCount}";
            ((System.Windows.Controls.Label)this.FindResource("DiscInfo")).Content = $"{currentTrack.DiscNumber} / {currentTrack.DiscCount}";
            ((System.Windows.Controls.Label)this.FindResource("Year")).Content = currentTrack.Year;
            ((System.Windows.Controls.Label)this.FindResource("Rating")).Content = currentTrack.Rating;
            ((System.Windows.Controls.Label)this.FindResource("Genres")).Content = String.Join("; ", currentTrack.Genres);
            ((System.Windows.Controls.Label)this.FindResource("BPM")).Content = currentTrack.BPM;
            ((System.Windows.Controls.Label)this.FindResource("Key")).Content = currentTrack.Key;

            BtnNext.IsEnabled = BtnPlayPause.IsEnabled = BtnPrevious.IsEnabled = TrackSlider.IsEnabled = true;
            if (isPlaying) Play();
        }

        public void UnloadTrack()
        {
            if (isPlaying) Pause();
            GridPlayer.Visibility = Visibility.Collapsed;
            BtnNext.IsEnabled = BtnPlayPause.IsEnabled = BtnPrevious.IsEnabled = TrackSlider.IsEnabled = false;
            TrackSlider.Value = 0;
            LabelTrackTime.Content = "";
            currentTrack = null;
        }

        private void TrackSlider_DragStarted(object sender, RoutedEventArgs e)
        {
            trackSliderBeingDragged = true;
            if (isPlaying) player.Pause();
        }
        
        private void TrackSlider_DragCompleted(object sender, RoutedEventArgs e)
        {
            trackSliderBeingDragged = false;
            if(isPlaying) player.Play();
        }

        private void TrackSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if(trackSliderBeingDragged) player.Position = TimeSpan.FromMilliseconds(TrackSlider.Value);
        }

        private void OnTrackLoad(object sender, EventArgs e)
        {
            TrackSlider.Maximum = player.NaturalDuration.TimeSpan.TotalMilliseconds;
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
            if (!trackSliderBeingDragged)
            {
                TrackSlider.Value = player.Position.TotalMilliseconds;
                while (!player.NaturalDuration.HasTimeSpan) { }
                LabelTrackTime.Content = $"{(player.Position).ToString(@"hh\:mm\:ss")} / {(player.NaturalDuration.TimeSpan).ToString(@"hh\:mm\:ss")}";
            }
        }

        private void Previous()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            timer.Stop();
            if (currentTrackIndex == 0)
            {
                LoadTrack(library.Tracks[library.Tracks.Count - 1]);
            }
            else
            {
                LoadTrack(library.Tracks[currentTrackIndex - 1]);
            }
        }

        private void Next()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            timer.Stop();
            if (currentTrackIndex + 1 == library.Tracks.Count)
            {
                LoadTrack(library.Tracks[0]);
            }
            else
            {
                LoadTrack(library.Tracks[currentTrackIndex + 1]);
            }
        }

        private void Play()
        {
            player.Play();
            while (timer == null) { }
            timer.Start();
            BtnPlayPause.Content = FindResource("Pause");
            isPlaying = true;
        }

        private void Pause()
        {
            player.Pause();
            timer.Stop();
            BtnPlayPause.Content = FindResource("Play");
            isPlaying = false;
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
    }
}
