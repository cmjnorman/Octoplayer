using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

#nullable enable

namespace OctoplayerBackend
{
    public class TreeViewToggleItem : INotifyPropertyChanged
    {
        public string Path { get; }
        public string Header => Path.Substring(Path.LastIndexOf("\\") + 1);
        private bool? isChecked;
        public bool? IsChecked 
        {
            get => isChecked;
            set => SetCheckedState(value, true, true);
        }
        public TreeViewToggleItem? Parent { get; }
        public List<TreeViewToggleItem> Children { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public TreeViewToggleItem(string path, bool initialState, TreeViewToggleItem? parent = null)
        {
            this.Path = path;
            this.Parent = parent;
            isChecked = initialState;
            this.Children = new List<TreeViewToggleItem>();
        }

        private void SetCheckedState(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == IsChecked) return;

            isChecked = value;

            if (updateChildren) Children.ForEach(c => c.SetCheckedState(value, true, false));
            if (updateParent && Parent != null) Parent.ComputeCheckedStateFromChildren();
            OnPropertyChanged("IsChecked");
        }

        private void ComputeCheckedStateFromChildren()
        {
            bool? state = null;
            if (Children.All(c => c.IsChecked == true)) state = true;
            else if (Children.All(c => c.IsChecked == false)) state = false;
            SetCheckedState(state, false, true);
        }

        private void OnPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
