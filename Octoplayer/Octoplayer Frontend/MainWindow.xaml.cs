using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TagLib;


namespace Octoplayer_Frontend
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private bool isPlaying = false;
        private TagLib.File track;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio files|*.mp3; *.flac; *.wav|All files|*.*";
            if(dialog.ShowDialog() == true)
            {
                Pause();
                txtFilePath.Text = dialog.FileName;
                track = TagLib.File.Create(dialog.FileName);
                player.Open(new Uri(dialog.FileName));

                lblTrackTitle.Content = track.Tag.Title;
                var artists = new StringBuilder();
                artists.Append(track.Tag.Performers[0]);
                foreach (var artist in track.Tag.Performers.Skip(1))
                {
                    artists.Append(artist);
                }
                lblArtists.Content = artists;
                lblAlbum.Content = track.Tag.Album;

                ((System.Windows.Controls.Label)this.FindResource("TrackInfo")).Content = $"{track.Tag.Track} / {track.Tag.TrackCount}";
                ((System.Windows.Controls.Label)this.FindResource("DiscInfo")).Content = $"{track.Tag.Disc} / {track.Tag.DiscCount}";
                ((System.Windows.Controls.Label)this.FindResource("Year")).Content = track.Tag.Year;
                ((System.Windows.Controls.Label)this.FindResource("Rating")).Content = "0";
                
                var genres = new StringBuilder();
                genres.Append(track.Tag.Genres[0]);
                foreach (var genre in track.Tag.Genres.Skip(1))
                {
                    artists.Append($"; {genre}");
                }
                ((System.Windows.Controls.Label)this.FindResource("Genres")).Content = genres;

                ((System.Windows.Controls.Label)this.FindResource("BPM")).Content = track.Tag.BeatsPerMinute;
                ((System.Windows.Controls.Label)this.FindResource("Key")).Content = track.Tag.InitialKey;

                gridControls.Visibility = Visibility.Visible;
                gridInfo.Visibility = Visibility.Visible;

                MemoryStream ms = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                imgAlbumArt.Source = bitmap;

                expander.IsExpanded = false;
            }
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
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void Play()
        {
            player.Play();
            btnPlayPause.Content = FindResource("Pause");
            isPlaying = true;
        }

        private void Pause()
        {
            player.Pause();
            btnPlayPause.Content = FindResource("Play");
            isPlaying = false;
        }

        //private System.Windows.Controls.Label SearchForLabelWithinElement(DependencyObject element, string name)
        //{
        //    for(int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(element, i);
        //        if ((child is System.Windows.Controls.Label))
        //        {
        //            var label = (System.Windows.Controls.Label)child;
        //            if (label.Name == name)
        //            {
        //                return label;
        //            }
        //        }
        //        if (VisualTreeHelper.GetChildrenCount(child) > 0)
        //        {
        //            var result = SearchForLabelWithinElement(child, name);
        //            if (result != null) return result;
        //        }
        //    }
        //    return null;
        //}

    }
}
