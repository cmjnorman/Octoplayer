using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
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

                MemoryStream ms = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                imgAlbumArt.Source = bitmap;
            }
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
    }
}
