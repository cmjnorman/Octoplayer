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
            //BtnBack.Visibility = Visibility.Collapsed;
            //BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
            if (library.Tracks.Count > 0)
            {
                LblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";
                //ListBoxTracks.ItemsSource = library.Tracks;
                //ListBoxAlbums.ItemsSource = library.Albums;
                //ListBoxArtists.ItemsSource = library.Artists;
                //ListBoxGenres.ItemsSource = library.Genres;
                LibraryBrowser.Visibility = Visibility.Visible;
                //SwitchListBox(ListBoxTracks);
                //BtnViewTracks.IsEnabled = false;
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
                //ListBoxTracks.ItemsSource = library.Tracks;
                //ListBoxAlbums.ItemsSource = library.Albums;
                //ListBoxArtists.ItemsSource = library.Artists;
                //ListBoxGenres.ItemsSource = library.Genres;
                LibraryBrowser.Visibility = Visibility.Visible;
                //SwitchListBox(ListBoxTracks);
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

        private Grid CreateBrowserPage<T>(List<T> items, Type? sourceType = null)
        {
            //Create ListBox to inhabit with passed items
            var list = new ListBox()
            {
                ItemsSource = items,
                ItemContainerStyle = (Style)this.FindResource("LibraryBrowserItemStyle"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            list.MouseDoubleClick += SelectBrowserItem;

            //Set item template for ListBox depending on the type of item contained
            if(sourceType == typeof(Album)) list.ItemTemplate = (DataTemplate)this.FindResource("AlbumTrackBrowserItemTemplate");
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

            //Create Grid to contain the listbox and possible buttons
            var page = new Grid();
            page.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            page.RowDefinitions.Add(new RowDefinition());

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
            
            page.Children.Add(list);
            Grid.SetRow(list, 1);
            return page;
        }

        private void TrackAlbumSwapButton_Click(object sender, RoutedEventArgs e)
        {
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LibraryBrowser.Children.Remove(browserPages.Pop());
            browserPages.Peek().Visibility = Visibility.Visible;
        }

        private void OpenBrowserPage<T>(List<T> items, Type? sourceType = null)
        {
            if(browserPages.Any()) browserPages.Peek().Visibility = Visibility.Collapsed;

            var page = CreateBrowserPage<T>(items, sourceType);
            LibraryBrowser.Children.Add(page);
            Grid.SetRow(page, 1);

            browserPages.Push(page);
        }

        private void CloseAllBrowserPages()
        {
            while (browserPages.Any())
            {
                LibraryBrowser.Children.Remove(browserPages.Pop());
            }
            
        }

        private void SelectBrowserView(object sender, RoutedEventArgs e)
        {
            CloseAllBrowserPages();
            switch ((string)((Button)e.Source).Content)
            {
                case "Tracks":
                    OpenBrowserPage<Track>(library.Tracks);
                    break;
                case "Albums":
                    OpenBrowserPage<Album>(library.Albums);
                    break;
                case "Artists":
                    OpenBrowserPage<Artist>(library.Artists);
                    break;
                case "Genres":
                    OpenBrowserPage<Genre>(library.Genres);
                    break;
            }
        }

        private void SelectBrowserItem(object sender, RoutedEventArgs e)
        {
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            //if (!BtnViewAlbums.IsEnabled)
            //{
            //    SwitchListBox(ListBoxAlbums);
            //    BtnBack.Visibility = Visibility.Collapsed;
            //    SwitchSearchBar(SearchBoxAlbums);
            //}
            //else if (!BtnViewArtists.IsEnabled)
            //{
            //    if (ListBoxAlbumTracks.Visibility == Visibility.Visible) SwitchListBox(ListBoxAlbumsSubmenu);
            //    else
            //    {
            //        SwitchListBox(ListBoxArtists);
            //        BtnSwapTrackAlbum.Visibility = Visibility.Collapsed;
            //        BtnBack.Visibility = Visibility.Collapsed;
            //        SwitchSearchBar(SearchBoxArtists);
            //    }
            //}
            //else if (!BtnViewGenres.IsEnabled)
            //{
            //    SwitchListBox(ListBoxGenres);
            //    BtnBack.Visibility = Visibility.Collapsed;
            //    SwitchSearchBar(SearchBoxGenres);
            //}
        }

        private void BtnSwapTrackAlbum_Click(object sender, RoutedEventArgs e)
        {
            //if((string)BtnSwapTrackAlbum.Content == "Show Artist Albums")
            //{
            //    SwitchListBox(ListBoxAlbumsSubmenu);
            //    BtnSwapTrackAlbum.Content = "Show Artist Tracks";
            //}
            //else
            //{
            //    SwitchListBox(ListBoxTracksSubmenu);
            //    BtnSwapTrackAlbum.Content = "Show Artist Albums";
            //}
        }

        //private void HideSearchBar()
        //{
        //    SearchBoxTracks.Visibility = SearchBoxAlbums.Visibility = SearchBoxArtists.Visibility = SearchBoxGenres.Visibility = Visibility.Collapsed;
        //}

        //private void SwitchSearchBar(System.Windows.Controls.TextBox box)
        //{
        //    foreach (var searchBox in LibraryBrowser.Children.OfType<System.Windows.Controls.TextBox>())
        //    {
        //        searchBox.Visibility = (searchBox == box ? Visibility.Visible : Visibility.Collapsed);
        //    }
        //}

        //private void SwitchListBox(ListBox box)
        //{
        //    foreach (var listBox in LibraryBrowser.Children.OfType<ListBox>())
        //    {
        //        listBox.Visibility = (listBox == box ? Visibility.Visible : Visibility.Collapsed);
        //    }
        //}

        //private void ListBoxTracks_Select(object sender, RoutedEventArgs e)
        //{
        //    if(ListBoxTracks.SelectedItem != null)
        //    {
        //        player.SelectTracks(ListBoxTracks.Items.OfType<Track>().ToList(), ListBoxTracks.SelectedIndex, ShuffleEnabled);
        //    }
        //}

        //private void ListBoxAlbums_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxAlbums.SelectedItem != null)
        //    {
        //        ListBoxAlbumTracks.ItemsSource = ((Album)ListBoxAlbums.SelectedItem).Tracks;
        //        SwitchListBox(ListBoxAlbumTracks);
        //        BtnBack.Visibility = Visibility.Visible;
        //        HideSearchBar();
        //    }
        //}

        //private void ListBoxArtists_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxArtists.SelectedItem != null)
        //    {
        //        var artist = (Artist)ListBoxArtists.SelectedItem;
        //        ListBoxTracksSubmenu.ItemsSource = artist.Tracks.Concat(artist.Remixes).OrderBy(a => a.Title);
        //        ListBoxAlbumsSubmenu.ItemsSource = ((Artist)ListBoxArtists.SelectedItem).Albums;
        //        SwitchListBox(ListBoxTracksSubmenu);
        //        BtnBack.Visibility = Visibility.Visible;
        //        BtnSwapTrackAlbum.Content = "Show Artist Albums";
        //        BtnSwapTrackAlbum.Visibility = Visibility.Visible;
        //        HideSearchBar();
        //    }
        //}

        //private void ListBoxGenres_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxGenres.SelectedItem != null)
        //    {
        //        ListBoxTracksSubmenu.ItemsSource = ((Genre)ListBoxGenres.SelectedItem).Tracks;
        //        SwitchListBox(ListBoxTracksSubmenu);
        //        BtnBack.Visibility = Visibility.Visible;
        //        HideSearchBar();
        //    }
        //}

        //private void ListBoxAlbumTracks_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxAlbumTracks.SelectedItem != null)
        //    {
        //        player.SelectTracks(ListBoxAlbumTracks.Items.OfType<Track>().ToList(), ListBoxAlbumTracks.SelectedIndex, ShuffleEnabled);
        //    }
        //}

        //private void ListBoxTracksSubmenu_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxTracksSubmenu.SelectedItem != null)
        //    {
        //        player.SelectTracks(ListBoxTracksSubmenu.Items.OfType<Track>().ToList(), ListBoxTracksSubmenu.SelectedIndex, ShuffleEnabled);
        //    }
        //}

        //private void ListBoxAlbumsSubmenu_Select(object sender, RoutedEventArgs e)
        //{
        //    if (ListBoxAlbumsSubmenu.SelectedItem != null)
        //    {
        //        ListBoxAlbumTracks.ItemsSource = ((Album)ListBoxAlbumsSubmenu.SelectedItem).Tracks;
        //        SwitchListBox(ListBoxAlbumTracks);
        //        BtnBack.Visibility = Visibility.Visible;
        //        HideSearchBar();
        //    }
        //}

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
            if (item.Content is Track && (Track)item.Content != player.Queue.CurrentTrack)
            {
                var menuItem = new MenuItem() { Header = "Add To Front Of Queue" };
                menuItem.Click += AddTrackToFront;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Add To Back Of Queue" };
                menuItem.Click += AddTrackToBack;
                menu.Items.Add(menuItem);
            }
            else if (item.Content is QueueItem && (Track)((QueueItem)item.Content).Track != player.Queue.CurrentTrack)
            {
                var menuItem = new MenuItem() { Header = "Move To Front" };
                menuItem.Click += MoveToFrontOfQueue;
                menu.Items.Add(menuItem);

                menuItem = new MenuItem() { Header = "Move To Back" };
                menuItem.Click += MoveToBackOfQueue;
                menu.Items.Add(menuItem);
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
            UpdateContextMenu((ListBoxItem)((ListBox)sender).ItemContainerGenerator.ContainerFromItem(((ListBox)sender).SelectedItem));
        }

        private void SearchBoxTracks_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var items = CollectionViewSource.GetDefaultView(ListBoxTracks.ItemsSource);
            //if (items != null)
            //{
            //    items.Filter = TrackSearch;
            //}
            //ListBoxTracks.ItemsSource = items;
        }

        private void SearchBoxAlbums_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var items = CollectionViewSource.GetDefaultView(ListBoxAlbums.ItemsSource);
            //if (items != null)
            //{
            //    items.Filter = AlbumSearch;
            //}
            //ListBoxAlbums.ItemsSource = items;
        }

        private void SearchBoxArtists_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var items = CollectionViewSource.GetDefaultView(ListBoxArtists.ItemsSource);
            //if (items != null)
            //{
            //    items.Filter = ArtistSearch;
            //}
            //ListBoxArtists.ItemsSource = items;
        }

        private void SearchBoxGenres_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var items = CollectionViewSource.GetDefaultView(ListBoxGenres.ItemsSource);
            //if (items != null)
            //{
            //    items.Filter = GenreSearch;
            //}
            //ListBoxGenres.ItemsSource = items;
        }

        //private bool TrackSearch(object item)
        //{
        //    var track = item as Track;
        //    if(track.Title.ToLower().Contains(SearchBoxTracks.Text.ToLower()) || track.Album.Title.ToLower().Contains(SearchBoxTracks.Text.ToLower()) || String.Join("; ", track.Artists).ToLower().Contains(SearchBoxTracks.Text.ToLower()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool AlbumSearch(object item)
        //{
        //    var album = item as Album;
        //    if (album.Title.ToLower().Contains(SearchBoxAlbums.Text.ToLower()) || String.Join("; ", album.Artists).ToLower().Contains(SearchBoxAlbums.Text.ToLower()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool ArtistSearch(object item)
        //{
        //    var artist = item as Artist;
        //    if (artist.Name.ToLower().Contains(SearchBoxArtists.Text.ToLower()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool GenreSearch(object item)
        //{
        //    var genre = item as Genre;
        //    if (genre.Name.ToLower().Contains(SearchBoxGenres.Text.ToLower()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        private void Window_Closed(object sender, EventArgs e)
        {
            player.LogData();
            library.SaveLibrary();
        }
    }
}
