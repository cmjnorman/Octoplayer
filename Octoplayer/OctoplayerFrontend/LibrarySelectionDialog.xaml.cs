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
        public LibrarySelectionDialog()
        {
            InitializeComponent();
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderViewer.Items.Add(GetTreeItem(folderBrowser.SelectedPath));
            }
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            var folders = FolderViewer.Items.OfType<TreeViewToggleItem>().RecursiveSelect(i => i.Children).Where(i => !i.Children.Any());
            var selected = folders.Where(i => i.IsChecked == true).Select(i => i.Path);
            ((MainWindow)Application.Current.MainWindow).SelectLibraryFolders(selected);
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private TreeViewToggleItem GetTreeItem(string path)
        {
            var item = new TreeViewToggleItem(path);
            item.Children = GetSubfolders(item);
            return item;
        }

        private List<TreeViewToggleItem> GetSubfolders(TreeViewToggleItem parent)
        {
            var subItems = new List<TreeViewToggleItem>();
            try
            {
                foreach (var path in Directory.GetDirectories(parent.Path))
                {
                    var item = new TreeViewToggleItem(path, parent);
                    item.Children = GetSubfolders(item);
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
