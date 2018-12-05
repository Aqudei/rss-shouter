using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using RSSLoudReader.Events;
using RSSLoudReader.Models;

namespace RSSLoudReader.ViewModels
{
    sealed class RssEntriesViewModel : Screen, IHandle<Events.RssFeedAdded>
    {
        public BindableCollection<RssEntry> Feeds { get; set; }
            = new BindableCollection<RssEntry>();

        public RssEntriesViewModel()
        {
            DisplayName = "RSS Entries";

            using (var db = new RssContext())
            {
                Feeds.AddRange(db.RssEntries.ToArray());
            }
        }

        public void Handle(RssFeedAdded message)
        {
            Feeds.Add(message.RssEntry);
        }
    }
}
