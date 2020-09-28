using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octoplayer_Backend;


namespace Octoplayer_Frontend
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private Track currentTrack;
        private bool isPlaying = false;
        private Library library;
        private string[] extensions = { ".mp3", ".wav", ".flac" };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();
            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pause();
                library = new Library();
                var files = Directory.GetFiles(folderBrowser.SelectedPath);
                foreach (var file in files)
                {
                    if (extensions.Contains(Path.GetExtension(file))) library.AddTrack(file);
                }
                currentTrack = library.Tracks[0];
                lblFilesLoaded.Content = $"{library.Tracks.Count} file{(library.Tracks.Count > 1 ? "s" : "")} loaded.";

                loadTrack();

                gridControls.Visibility = Visibility.Visible;
                gridInfo.Visibility = Visibility.Visible;
            }
        }

        private void loadTrack()
        {
            player.Open(new Uri(currentTrack.FilePath));

            lblTrackTitle.Content = currentTrack.Title;
            var artists = new StringBuilder();
            artists.Append(currentTrack.Artists[0]);
            foreach (var artist in currentTrack.Artists.Skip(1))
            {
                artists.Append(artist);
            }
            lblArtists.Content = artists;
            lblAlbum.Content = currentTrack.Album;

            ((System.Windows.Controls.Label)this.FindResource("TrackInfo")).Content = $"{currentTrack.TrackNumber} / {currentTrack.TrackCount}";
            ((System.Windows.Controls.Label)this.FindResource("DiscInfo")).Content = $"{currentTrack.DiscNumber} / {currentTrack.DiscCount}";
            ((System.Windows.Controls.Label)this.FindResource("Year")).Content = currentTrack.Year;
            ((System.Windows.Controls.Label)this.FindResource("Rating")).Content = currentTrack.Rating;

            var genres = new StringBuilder();
            genres.Append(currentTrack.Genres[0]);
            foreach (var genre in currentTrack.Genres.Skip(1))
            {
                artists.Append($"; {genre}");
            }
                ((System.Windows.Controls.Label)this.FindResource("Genres")).Content = genres;

            ((System.Windows.Controls.Label)this.FindResource("BPM")).Content = currentTrack.BPM;
            ((System.Windows.Controls.Label)this.FindResource("Key")).Content = currentTrack.Key;

            MemoryStream ms = new MemoryStream(currentTrack.Artwork.Data.Data);
            ms.Seek(0, SeekOrigin.Begin);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            imgAlbumArt.Source = bitmap;

            expander.IsExpanded = false;
            player.MediaEnded += songEnd;
            if (isPlaying) play();
        }

        private void expander_expand(object sender, RoutedEventArgs e)
        {
            imgAlbumArt.Visibility = Visibility.Collapsed;
        }


        private void expander_collapse(object sender, RoutedEventArgs e)
        {
            imgAlbumArt.Visibility = Visibility.Visible;
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                pause();
            }
            else
            {
                play();
            }
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (player.Position.TotalSeconds >= 5) player.Position = new TimeSpan(0);
            else previous();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            next();
        }

        private void songEnd(object sender, EventArgs e)
        {
            next();
        }

        private void previous()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            if (currentTrackIndex == 0) currentTrack = library.Tracks[library.Tracks.Count - 1];
            else currentTrack = library.Tracks[currentTrackIndex - 1];
            loadTrack();
        }

        private void next()
        {
            var currentTrackIndex = library.Tracks.FindIndex(t => t.FilePath == currentTrack.FilePath);
            if (currentTrackIndex + 1 == library.Tracks.Count) currentTrack = library.Tracks[0];
            else currentTrack = library.Tracks[currentTrackIndex + 1];
            loadTrack();
        }

        private void play()
        {
            player.Play();
            btnPlayPause.Content = FindResource("Pause");
            isPlaying = true;
        }

        private void pause()
        {
            player.Pause();
            btnPlayPause.Content = FindResource("Play");
            isPlaying = false;
        }

        

        
    }
}
