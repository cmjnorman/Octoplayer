using OctoplayerBackend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OctoplayerFrontend
{
    /// <summary>
    /// Interaction logic for LibrarySelectionDialog.xaml
    /// </summary>
    public partial class LibrarySelectionDialog : Window
    {
        public LibrarySelectionDialog(List<string> libraryFolders)
        {
            InitializeComponent();
            FolderViewer.Items.Add(GetTreeView(GetHighestCommonPath(libraryFolders), false));
            SelectActiveFolders(libraryFolders);
        }

        private string GetHighestCommonPath(List<string> folderPaths)
        {
            var splitPath = folderPaths.First().Split("\\");
            string highestCommonPath = splitPath[0];
            for(var i = 1; i < splitPath.Length; i++)
            {
                if (folderPaths.All(p => p.Split("\\")[i] == splitPath[i])) highestCommonPath += $"\\{splitPath[i]}";
                else break;
            }
            return highestCommonPath;
        }

        private void SelectActiveFolders(List<string> folderPaths)
        {
            var folders = FolderViewer.Items.OfType<TreeViewToggleItem>().RecursiveSelect(i => i.Children).Where(i => !i.Children.Any());
            foreach(var path in folderPaths)
            {
                folders.First(f => f.Path == path).IsChecked = true;
            }
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderViewer.Items.Add(GetTreeView(folderBrowser.SelectedPath, true));
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            var folders = FolderViewer.Items.OfType<TreeViewToggleItem>().RecursiveSelect(i => i.Children).Where(i => !i.Children.Any());
            var selectedFolders = folders.Where(i => i.IsChecked == true).Select(i => i.Path);
            var files = new List<string>();
            foreach (var folder in selectedFolders)
            {
                files.AddRange(Directory.EnumerateFiles(folder).Where(f => f.EndsWith(".mp3") || f.EndsWith(".flac")).ToList());
            }
            ((MainWindow)Application.Current.MainWindow).SelectLibraryFiles(files, selectedFolders.ToList());
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private TreeViewToggleItem GetTreeView(string path, bool initialState)
        {
            var item = new TreeViewToggleItem(path, initialState);
            item.Children = GetSubfolders(item, initialState);
            return item;
        }

        private List<TreeViewToggleItem> GetSubfolders(TreeViewToggleItem parent, bool initialState)
        {
            var subItems = new List<TreeViewToggleItem>();
            try
            {
                foreach (var path in Directory.GetDirectories(parent.Path))
                {
                    var item = new TreeViewToggleItem(path, initialState, parent);
                    item.Children = GetSubfolders(item, initialState);
                    subItems.Add(item);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
            return subItems;
        }
    }
}
