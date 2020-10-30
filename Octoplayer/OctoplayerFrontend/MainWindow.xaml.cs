using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;
using OctoplayerBackend;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

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
            player.QueueUpdated += OnQueueUpdated;
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
                player.SelectTracks(ListBoxTracks.Items.OfType<Track>().ToList(), ListBoxTracks.SelectedIndex, ShuffleEnabled);
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
                player.SelectTracks(ListBoxAlbumTracks.Items.OfType<Track>().ToList(), ListBoxAlbumTracks.SelectedIndex, ShuffleEnabled);
            }
        }

        private void ListBoxTracksSubmenu_Select(object sender, RoutedEventArgs e)
        {
            if (ListBoxTracksSubmenu.SelectedItem != null)
            {
                player.SelectTracks(ListBoxTracksSubmenu.Items.OfType<Track>().ToList(), ListBoxTracksSubmenu.SelectedIndex, ShuffleEnabled);
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

            ((System.Windows.Controls.TextBlock)this.FindResource("Title")).Text = track.Title; 
            ((System.Windows.Controls.TextBlock)this.FindResource("Artists")).Text = String.Join("; ", track.Artists);
            ((System.Windows.Controls.TextBlock)this.FindResource("Album")).Text = track.Album.Title;
            ((System.Windows.Controls.TextBlock)this.FindResource("TrackInfo")).Text = $"{track.TrackNumber} / {track.TrackCount}";
            ((System.Windows.Controls.TextBlock)this.FindResource("DiscInfo")).Text = $"{track.DiscNumber} / {track.DiscCount}";
            ((System.Windows.Controls.TextBlock)this.FindResource("Year")).Text = track.Year.ToString();
            ((System.Windows.Controls.TextBlock)this.FindResource("Rating")).Text = track.Rating.ToString();
            ((System.Windows.Controls.TextBlock)this.FindResource("Genres")).Text = String.Join("; ", track.Genres);
            ((System.Windows.Controls.TextBlock)this.FindResource("BPM")).Text = track.BPM.ToString();
            ((System.Windows.Controls.TextBlock)this.FindResource("Key")).Text = track.Key;
            ImgAlbumArt.Source = track.Artwork;

            BtnNext.IsEnabled = BtnPlayPause.IsEnabled = BtnPrevious.IsEnabled = TrackSlider.IsEnabled = true;
            TrackSlider.Maximum = player.CurrentTrackLength;
            timelineClock = new DispatcherTimer();
            timelineClock.Interval = TimeSpan.FromMilliseconds(1);
            timelineClock.Tick += timelineClock_Tick;
            timelineClock.Start();
        }

        private void OnQueueUpdated()
        {
            var queue = player.Queue.GetQueueItems();
            NextTrackItemControl.ItemsSource = queue.Where(q => q.RelativePosition == 1);
            QueueListBox.ItemsSource = queue;
        }

        private void UpdateContextMenu(ListBoxItem item)
        {
            var menu = new ContextMenu();
            if (item.Content is Track)
                if (player.Queue.GetQueueItems().FirstOrDefault(q => q.Track == item.Content) == null)
                {
                    var menuItem = new MenuItem() { Header = "Add To Front Of Queue" };
                    menuItem.Click += MenuItemAddToFrontOfQueue_Click;
                    menu.Items.Add(menuItem);

                    menuItem = new MenuItem() { Header = "Add To Back Of Queue" };
                    menuItem.Click += MenuItemAddToBackOfQueue_Click;
                    menu.Items.Add(menuItem);
                }
                else
                {
                    var menuItem = new MenuItem() { Header = "Move To Front Of Queue" };
                    menuItem.Click += MenuItemMoveToFrontOfQueue_Click;
                    menu.Items.Add(menuItem);

                    menuItem = new MenuItem() { Header = "Move To Back Of Queue" };
                    menuItem.Click += MenuItemMoveToBackOfQueue_Click;
                    menu.Items.Add(menuItem);
                }
            item.ContextMenu = menu;
        }

        private void UnloadTrack()
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
            BtnShuffle.Tag = (ShuffleEnabled ? "On" : "Off");
            if (ShuffleEnabled) player.ShuffleQueue();
            else player.UnshuffleQueue();
        }

        private void QueueListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            player.SkipTo(((QueueItem)QueueListBox.SelectedItem).RelativePosition);
        }

        private void QueueToggle_Click(object sender, RoutedEventArgs e)
        {
            QueueListBox.ScrollIntoView(QueueListBox.Items.GetItemAt(QueueListBox.Items.Count - 1));
            QueueListBox.UpdateLayout();
            QueueListBox.ScrollIntoView(QueueListBox.Items.GetItemAt(player.Queue.TopScrollPosition()));
        }

        private void MenuItemAddToFrontOfQueue_Click(object sender, RoutedEventArgs e)
        {
            player.AddTrack((Track)((MenuItem)e.Source).DataContext, true, ShuffleEnabled);
        }

        private void MenuItemAddToBackOfQueue_Click(object sender, RoutedEventArgs e)
        {
            player.AddTrack((Track)((MenuItem)e.Source).DataContext, false, ShuffleEnabled);
        }

        private void MenuItemMoveToFrontOfQueue_Click(object sender, RoutedEventArgs e)
        {
            player.MoveTrackPosition((Track)((MenuItem)e.Source).DataContext, true);
        }
        private void MenuItemMoveToBackOfQueue_Click(object sender, RoutedEventArgs e)
        {
            player.MoveTrackPosition((Track)((MenuItem)e.Source).DataContext, false);
        }

        private void ListBoxTracks_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateContextMenu((ListBoxItem)ListBoxTracks.ItemContainerGenerator.ContainerFromItem(ListBoxTracks.SelectedItem));
        }
    }
}
