using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace OctoplayerBackend
{
    public interface IBrowsable
    {
        public string Heading { get; }
        public string SubHeading1 { get; }
        public string SubHeading2 { get; }
        public BitmapImage Image { get; }
    }
}
