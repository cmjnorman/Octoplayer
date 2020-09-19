using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;


namespace Octoplayer_Frontend
{
    public partial class MainWindow : Window
    {
        private MediaPlayer player = new MediaPlayer();
        private bool isPlaying = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio files|*.mp3; *.flac; *.wav |All files (*.*)|*.*";
            if(dialog.ShowDialog() == true)
            {
                Pause();
                txtFilePath.Text = dialog.FileName;
                player.Open(new Uri(dialog.FileName));
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
