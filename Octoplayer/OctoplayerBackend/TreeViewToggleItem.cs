using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OctoplayerBackend
{
    public class TreeViewToggleItem 
    {
        public string Path { get; }
        public string Header => Path.Substring(Path.LastIndexOf("\\") + 1);
        private bool? isChecked;
        public bool? IsChecked 
        {
            get => isChecked;
            set => SetCheckedState(value, true, true);
        }
        private TreeViewToggleItem parent;
        public List<TreeViewToggleItem> Children { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TreeViewToggleItem(string path, TreeViewToggleItem parent = null)
        {
            this.Path = path;
            this.parent = parent;
            this.Children = new List<TreeViewToggleItem>();
        }

        private void SetCheckedState(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == IsChecked) return;

            isChecked = value;

            if (updateChildren) Children.ForEach(c => c.SetCheckedState(value, true, false));
            if (updateParent && parent != null) parent.ComputeCheckedStateFromChildren();
            //OnPropertyChanged("IsChecked");
        }

        private void ComputeCheckedStateFromChildren()
        {
            bool? state = null;
            if (Children.All(c => c.IsChecked == true)) state = true;
            else if (Children.All(c => c.IsChecked == false)) state = false;
            SetCheckedState(state, false, true);
        }

        //private void OnPropertyChanged(string property)
        //{
        //    if(PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(property));
        //    }
        //}
    }
}
