using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using RSSLoudReader.Events;
using RSSLoudReader.Models;

namespace RSSLoudReader.ViewModels
{
    sealed class RssEntriesViewModel : Screen, IHandleWithTask<Events.RssFeedAdded>
    {
        private readonly IEventAggregator _aggregator;

        private readonly BindableCollection<RssEntry> _feeds = new BindableCollection<RssEntry>();
        private string _filterText;

        public ICollectionView Feeds { get; set; }

        public RssEntriesViewModel(IEventAggregator aggregator)
        {
            Feeds = CollectionViewSource.GetDefaultView(_feeds);

            _aggregator = aggregator;
            DisplayName = "RSS Entries";

            using (var db = new RssContext())
            {
                _feeds.AddRange(db.RssEntries.ToArray());
            }

            aggregator.Subscribe(this);

            Feeds.SortDescriptions.Add(new SortDescription("PublishedDate",
                ListSortDirection.Descending));

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(FilterText))
                    return;

                if (string.IsNullOrWhiteSpace(FilterText))
                {
                    Feeds.Filter = o => true;
                }
                else if (FilterText.Length < 3)
                {
                    Feeds.Filter = o => false;
                }
                else
                {
                    Feeds.Filter = o => o is RssEntry item && (item.PublishedDate.ToString(CultureInfo.InvariantCulture).Contains(FilterText) ||
                                                               item.Title.ToLower().Contains(FilterText.ToLower()) ||
                                                               item.Url.ToLower().Contains(FilterText.ToLower()));
                }
            };
        }

        public void OpenRss(RssEntry rssEntry)
        {
            Process.Start(rssEntry.Url);
        }

        public void DeleteRss(RssEntry rssEntry)
        {
            using (var db = new RssContext())
            {
                var item = db.RssEntries.Find(rssEntry.Id);
                if (item == null)
                    return;
                db.RssEntries.Remove(item);
                db.SaveChanges();
                _feeds.Remove(rssEntry);

            }
        }

        public Task Handle(RssFeedAdded message)
        {
            return Task.Run(() => _feeds.Add(message.RssEntry));
        }

        public string FilterText
        {
            get => _filterText;
            set => Set(ref _filterText, value);
        }
    }
}
