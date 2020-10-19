using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OctoplayerBackend
{
    public class Browser
    {
        public Library Library { get; set; }
        public List<IBrowsable> Items { get; set; }

        private BrowserItemType mode;

        public Browser(Library library, BrowserItemType initialMode = BrowserItemType.Tracks)
        {
            this.Library = library;
            ChangeMode(initialMode);
        }

        public void ChangeMode(BrowserItemType mode)
        {
            switch (mode)
            {
                case BrowserItemType.Tracks:
                    this.Items = Library.Tracks.Cast<IBrowsable>().ToList();
                    break;
                case BrowserItemType.Albums:
                    this.Items = Library.Albums.Cast<IBrowsable>().ToList();
                    break;
                case BrowserItemType.Artists:
                    this.Items = Library.Artists.Cast<IBrowsable>().ToList();
                    break;
                case BrowserItemType.Genres:
                    this.Items = Library.Genres.Cast<IBrowsable>().ToList();
                    break;
            }
        }
    }
}
