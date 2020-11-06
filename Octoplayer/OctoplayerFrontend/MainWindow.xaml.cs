using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OctoplayerBackend;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Net.WebSockets;

namespace OctoplayerFrontend
{
    public partial class MainWindow : Window
    {
        private Player player;
        private DispatcherTimer timelineClock;
        private Library library;
        private Stack<Grid> browserPages;
        private bool trackSliderBeingDragged = false;
        private bool ShuffleEnabled = false;


        public MainWindow()
        {
            InitializeComponent();
            library = new Library();
            this.player = new Player(library);
            player.TrackLoaded += OnTrackLoad;
            player.QueueUpdated += OnQueueUpdated;
            player.MediaPlaying += () => BtnPlayPause.Content = FindResource("Pause");
            player.MediaPaused += () => BtnPlayPause.Content = FindResource("Play");
            browserPages = new Stack<Grid>();
            GridPlayer.Visibility = Visibility.Hidden;
            LibraryBrowser.Visibility = Visibility.Hidden;
            if (library.Tracks.Count > 0)
            {
                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                LibraryBrowser.Visibility = Visibility.Visible;
                OpenBrowserPage(library.Tracks);
            }
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(player.IsPlaying) player.Pause();
                UnloadTrack();

                library = new Library(Directory.GetFiles(folderBrowser.SelectedPath, "*", SearchOption.AllDirectories));

                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";

                LibraryBrowser.Visibility = Visibility.Visible;
                OpenBrowserPage(library.Tracks);
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

        private Grid CreateBrowserPage<T>(List<T> items, Type? sourceType = null)
        {
            //Create ListBox to occupy with passed items List parameter
            var list = new ListBox()
            {
                ItemsSource = items,
                ItemContainerStyle = (Style)this.FindResource("LibraryBrowserItemStyle"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            list.MouseDoubleClick += SelectBrowserItem;
            list.MouseRightButtonUp += ListBoxRightClick;

            //Set item template for ListBox depending on the type of item contained
            if (sourceType == typeof(Album)) list.ItemTemplate = (DataTemplate)this.FindResource("AlbumTrackBrowserItemTemplate");
            else
            {
                switch (typeof(T))
                {
                    case var track when track == typeof(Track):
                    case var album when album == typeof(Album):
                        list.ItemTemplate = (DataTemplate)this.FindResource("TrackAndAlbumBrowserItemTemplate");
                        break;
                    case var artist when artist == typeof(Artist):
                    case var genre when genre == typeof(Genre):
                        list.ItemTemplate = (DataTemplate)this.FindResource("ArtistAndGenreBrowserItemTemplate");
                        break;
                }
            }

            //Create Grid to contain the listbox and possible buttons or searchbar
            var page = new Grid();
            page.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            page.RowDefinitions.Add(new RowDefinition());

            //If browser is at top level (no pages already open), insert searchbar to filter list
            if (!browserPages.Any())
            {
                var searchBar = new TextBox() { Style = (Style)this.FindResource("LibrarySearchBar"), Height = 30, Margin = new Thickness(5, 10, 5, 10) };
                searchBar.TextChanged += SearchBar_TextChanged;
                page.Children.Add(searchBar);
            }

            //If a browser page is already open, insert back button to return to said page
            if(browserPages.Any())
            {
                var backButton = new Button() { Content="Back", Style = (Style)this.FindResource("RoundedButton"), Margin = new Thickness(5, 10, 0, 10), HorizontalAlignment = HorizontalAlignment.Left };
                backButton.Click += BackButton_Click;
                page.Children.Add(backButton);
            }

            //If source type is of Artist type, insert button to toggle between viewing artist tracks and albums
            if (sourceType == typeof(Artist))
            {
                var trackAlbumSwapButton = new Button() { Style = (Style)this.FindResource("RoundedButton"), Margin = new Thickness(0, 10, 5, 10), HorizontalAlignment = HorizontalAlignment.Right };
                trackAlbumSwapButton.Content = (typeof(T) == typeof(Track) ? "View Artist Albums" : "View Artist Tracks");
                trackAlbumSwapButton.Click += TrackAlbumSwapButton_Click;
                page.Children.Add(trackAlbumSwapButton);
            }
            
            //Insert ListBox
            page.Children.Add(list);
            Grid.SetRow(list, 1);

            return page;
        }

        private void OpenBrowserPage<T>(List<T> items, Type? sourceType = null)
        {
            //Hides current page and creates and opens new page
            if(browserPages.Any()) browserPages.Peek().Visibility = Visibility.Collapsed;

            var page = CreateBrowserPage<T>(items, sourceType);
            LibraryBrowser.Children.Add(page);
            Grid.SetRow(page, 1);

            browserPages.Push(page);
            ToggleBrowserViewButtons();
        }

        private void CloseAllBrowserPages()
        {
            //Deletes browsing history
            while (browserPages.Any())
            {
                LibraryBrowser.Children.Remove(browserPages.Pop());
            }
        }

        private void SelectBrowserView(object sender, RoutedEventArgs e)
        {
            //Switches highest level of browser between displaying Tracks, Albums, Artists, or Genres
            CloseAllBrowserPages();
            switch ((string)((Button)e.Source).Content)
            {
                case "Tracks":
                    ((TextBox)this.FindResource("SearchPromptText")).Text = "Search Tracks...";
                    OpenBrowserPage<Track>(library.Tracks);
                    break;
                case "Albums":
                    ((TextBox)this.FindResource("SearchPromptText")).Text = "Search Albums...";
                    OpenBrowserPage<Album>(library.Albums);
                    break;
                case "Artists":
                    ((TextBox)this.FindResource("SearchPromptText")).Text = "Search Artists...";
                    OpenBrowserPage<Artist>(library.Artists);
                    break;
                case "Genres":
                    ((TextBox)this.FindResource("SearchPromptText")).Text = "Search Genres...";
                    OpenBrowserPage<Genre>(library.Genres);
                    break;
            }
        }

        private void SelectBrowserItem(object sender, RoutedEventArgs e)
        {
            //Checks type of selected item, and executes respective methods
            var items = ((ListBox)e.Source).Items;
            var selectedItem = ((ListBox)e.Source).SelectedItem;
            if (selectedItem != null)
            {
                switch(selectedItem)
                {
                    case Track track:
                        player.SelectTracks(items.OfType<Track>().ToList(), items.IndexOf(track), ShuffleEnabled);
                        break;
                    case Album album:
                        OpenBrowserPage<Track>(album.Tracks, typeof(Album));
                        break;
                    case Artist artist:
                        OpenBrowserPage<Track>(artist.Tracks.Concat(artist.Remixes).ToList(), typeof(Artist));
                        break;
                    case Genre genre:
                        OpenBrowserPage<Track>(genre.Tracks);
                        break;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //Goes back to previous page
            LibraryBrowser.Children.Remove(browserPages.Pop());
            browserPages.Peek().Visibility = Visibility.Visible;
            ToggleBrowserViewButtons();
        }

        private void ToggleBrowserViewButtons()
        {
            //Checks weather all items contained in the library of each type are being displayed, and if so, disables and highlights textbox
            BtnViewTracks.IsEnabled = BtnViewAlbums.IsEnabled = BtnViewArtists.IsEnabled = BtnViewGenres.IsEnabled = true;
            var items = browserPages.Peek().Children.OfType<ListBox>().First().Items;
            if(items.OfType<Track>().Count() == library.Tracks.Count)
            {
                BtnViewTracks.IsEnabled = false;
            }
            if (items.OfType<Album>().Count() == library.Albums.Count)
            {
                BtnViewAlbums.IsEnabled = false;
            }
            if (items.OfType<Artist>().Count() == library.Artists.Count)
            {
                BtnViewArtists.IsEnabled = false;
            }
            if (items.OfType<Genre>().Count() == library.Genres.Count)
            {
                BtnViewGenres.IsEnabled = false;
            }
        }

        private void TrackAlbumSwapButton_Click(object sender, RoutedEventArgs e)
        {
            //Checks type of currently shown items, and creates a new browser with the opposite of what is currently shown (Tracks -> albums, albums -> tracks)
            browserPages.Peek().Visibility = Visibility.Collapsed;
            var items = browserPages.Pop().Children.OfType<ListBox>().First().Items;
            var tracks = items.OfType<Track>();
            var albums = items.OfType<Album>();
            if (tracks.Any())
            {
                OpenBrowserPage(tracks.Select(t => t.Album).Distinct().OrderBy(a => a.Title).ToList(), typeof(Artist));
            }
            if (albums.Any())
            {
                OpenBrowserPage(albums.SelectMany(a => a.Tracks).OrderBy(t => t.Title).ToList(), typeof(Artist));
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Checks browser view buttons to determine what items are being displayed, and returns a filtered list of those items using text entered in searchbar
            var searchBar = ((TextBox)e.Source);
            var listBox = browserPages.Peek().Children.OfType<ListBox>().First();
            if(!BtnViewTracks.IsEnabled)
            {
                var filteredTracks = new List<Track>();
                foreach(var track in library.Tracks)
                {
                    if (track.Title.ToLower().Contains(searchBar.Text.ToLower())) filteredTracks.Add(track);
                }
                listBox.ItemsSource = filteredTracks;
            }
            if (!BtnViewAlbums.IsEnabled)
            {
                var filteredAlbums = new List<Album>();
                foreach (var album in library.Albums)
                {
                    if (album.Title.ToLower().Contains(searchBar.Text.ToLower())) filteredAlbums.Add(album);
                }
                listBox.ItemsSource = filteredAlbums;
            }
            if (!BtnViewArtists.IsEnabled)
            {
                var filteredArtists = new List<Artist>();
                foreach (var artist in library.Artists)
                {
                    if (artist.Name.ToLower().Contains(searchBar.Text.ToLower())) filteredArtists.Add(artist);
                }
                listBox.ItemsSource = filteredArtists;
            }
            if (!BtnViewGenres.IsEnabled)
            {
                var filteredGenres = new List<Genre>();
                foreach (var genre in library.Genres)
                {
                    if (genre.Name.ToLower().Contains(searchBar.Text.ToLower())) filteredGenres.Add(genre);
                }
                listBox.ItemsSource = filteredGenres;
            }
        }


        private void OnTrackLoad()
        {
            if (GridPlayer.Visibility == Visibility.Hidden) GridPlayer.Visibility = Visibility.Visible;

            var track = player.Queue.CurrentTrack;

            ((TextBlock)this.FindResource("Title")).Text = track.Title; 
            ((TextBlock)this.FindResource("Artists")).Text = String.Join("; ", track.Artists);
            ((TextBlock)this.FindResource("Album")).Text = track.Album.Title;
            ((TextBlock)this.FindResource("TrackInfo")).Text = $"{track.TrackNumber} / {track.TrackCount}";
            ((TextBlock)this.FindResource("DiscInfo")).Text = $"{track.DiscNumber} / {track.DiscCount}";
            ((TextBlock)this.FindResource("Remixers")).Text = String.Join("; ", track.Remixers);
            ((TextBlock)this.FindResource("Year")).Text = track.Year.ToString();
            ((TextBlock)this.FindResource("Rating")).Text = track.Rating.ToString();
            ((TextBlock)this.FindResource("Genres")).Text = String.Join("; ", track.Genres);
            ((TextBlock)this.FindResource("BPM")).Text = track.BPM.ToString();
            ((TextBlock)this.FindResource("Key")).Text = track.Key;
            ImgAlbumArt.Source = track.Artwork;

            RemixersInfo.Visibility = (track.Remixers.Count > 0 ? Visibility.Visible : Visibility.Collapsed);

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
            {
                var track = (Track)item.Content;
                var menuItem = new MenuItem() { Header = "Go To Album" };
                menuItem.Click += GoToAlbum;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Go To Artist" };
                foreach (var artist in track.Artists.Concat(track.Remixers))
                {
                    var submenuItem = new MenuItem() { Header = artist.Name };
                    submenuItem.Click += GoToArtist;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Go To Genre" };
                foreach (var genre in track.Genres)
                {
                    var submenuItem = new MenuItem() { Header = genre.Name };
                    submenuItem.Click += GoToGenre;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                if (track != player.Queue.CurrentTrack)
                {
                    menuItem = new MenuItem() { Header = "Add To Front Of Queue" };
                    menuItem.Click += AddTrackToFront;
                    menu.Items.Add(menuItem);

                    menuItem = new MenuItem() { Header = "Add To Back Of Queue" };
                    menuItem.Click += AddTrackToBack;
                    menu.Items.Add(menuItem);
                }
            }
            else if (item.Content is Album)
            {
                var album = (Album)item.Content;
                var menuItem = new MenuItem() { Header = "Go To Artist" };
                foreach (var artist in album.Artists)
                {
                    var submenuItem = new MenuItem() { Header = artist.Name };
                    submenuItem.Click += GoToArtist;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Go To Genre" };
                foreach (var genre in album.Tracks.SelectMany(t => t.Genres))
                {
                    var submenuItem = new MenuItem() { Header = genre.Name };
                    submenuItem.Click += GoToGenre;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Add To Front Of Queue" };
                menuItem.Click += AddTracksToFront;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Add To Back Of Queue" };
                menuItem.Click += AddTracksToBack;
                menu.Items.Add(menuItem);
            }
            else if (item.Content is QueueItem)
            {
                var track = ((QueueItem)item.Content).Track;
                var menuItem = new MenuItem() { Header = "Go To Album" };
                menuItem.Click += GoToAlbum;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Go To Artist" };
                foreach (var artist in track.Artists.Concat(track.Remixers))
                {
                    var submenuItem = new MenuItem() { Header = artist.Name };
                    submenuItem.Click += GoToArtist;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Go To Genre" };
                foreach (var genre in track.Genres)
                {
                    var submenuItem = new MenuItem() { Header = genre.Name };
                    submenuItem.Click += GoToGenre;
                    menuItem.Items.Add(submenuItem);
                }
                menu.Items.Add(menuItem);

                if(((QueueItem)item.Content).Track != player.Queue.CurrentTrack)
                {
                    menuItem = new MenuItem() { Header = "Move To Front" };
                    menuItem.Click += MoveToFrontOfQueue;
                    menu.Items.Add(menuItem);

                    menuItem = new MenuItem() { Header = "Move To Back" };
                    menuItem.Click += MoveToBackOfQueue;
                    menu.Items.Add(menuItem);
                }
            }
            else
            {
                var menuItem = new MenuItem() { Header = "Add To Front Of Queue" };
                menuItem.Click += AddTracksToFront;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Add To Back Of Queue" };
                menuItem.Click += AddTracksToBack;
                menu.Items.Add(menuItem);
            }
            item.ContextMenu = menu;
        }

        private void GoToArtist(object sender, RoutedEventArgs e)
        {
            var artist = library.Artists.First(a => a.Name == (string)((MenuItem)e.Source).Header);
            OpenBrowserPage(artist.Tracks.Concat(artist.Remixes).ToList(), typeof(Artist));
        }

        private void GoToAlbum(object sender, RoutedEventArgs e)
        {
            Album album;
            if(((MenuItem)e.Source).DataContext is Track)
            {
                album = ((Track)((MenuItem)e.Source).DataContext).Album;
            }
            else
            {
                album = ((QueueItem)((MenuItem)e.Source).DataContext).Track.Album;
            }
           
            OpenBrowserPage(album.Tracks, typeof(Album));
        }

        private void GoToGenre(object sender, RoutedEventArgs e)
        {
            var genre = library.Genres.First(a => a.Name == (string)((MenuItem)e.Source).Header);
            OpenBrowserPage(genre.Tracks);
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

        private void AddTracksToFront(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)e.Source).DataContext is Album)
            {
                var tracks = ((Album)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, true, ShuffleEnabled);
                }
            }
            else if (((MenuItem)e.Source).DataContext is Artist)
            {
                var tracks = ((Artist)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, true, ShuffleEnabled);
                }
            }
            else
            {
                var tracks = ((Genre)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, true, ShuffleEnabled);
                }
            }
        }

        private void AddTracksToBack(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)e.Source).DataContext is Album)
            {
                var tracks = ((Album)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, false, ShuffleEnabled);
                }
            }
            else if (((MenuItem)e.Source).DataContext is Artist)
            {
                var tracks = ((Artist)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, false, ShuffleEnabled);
                }
            }
            else
            {
                var tracks = ((Genre)((MenuItem)e.Source).DataContext).Tracks;
                tracks.Reverse();
                foreach (var track in tracks)
                {
                    player.AddTrack(track, false, ShuffleEnabled);
                }
            }
        }

        private void AddTrackToFront(object sender, RoutedEventArgs e)
        {
            player.AddTrack((Track)((MenuItem)e.Source).DataContext, true, ShuffleEnabled);
        }

        private void AddTrackToBack(object sender, RoutedEventArgs e)
        {
            player.AddTrack((Track)((MenuItem)e.Source).DataContext, false, ShuffleEnabled);
        }

        private void MoveToFrontOfQueue(object sender, RoutedEventArgs e)
        {
            player.AddTrack(((QueueItem)((MenuItem)e.Source).DataContext).Track, true, ShuffleEnabled);
        }
        private void MoveToBackOfQueue(object sender, RoutedEventArgs e)
        {
            player.AddTrack(((QueueItem)((MenuItem)e.Source).DataContext).Track, false, ShuffleEnabled);
        }

        private void ListBoxRightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null) UpdateContextMenu((ListBoxItem)((ListBox)sender).ItemContainerGenerator.ContainerFromItem(((ListBox)sender).SelectedItem));
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            player.LogData();
            library.SaveLibrary();
        }
    }
}
