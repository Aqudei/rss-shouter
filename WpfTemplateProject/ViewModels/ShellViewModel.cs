using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Caliburn.Micro;
using RSSLoudReader.Events;
using RSSLoudReader.Models;

namespace RSSLoudReader.ViewModels
{
    sealed class ShellViewModel : Conductor<object>.Collection.OneActive, IHandle<Events.RssSourcesUpdatedEvent>
    {
        private readonly IEventAggregator _aggregator;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly List<RssSource> _rssSources = new List<RssSource>();

        public ShellViewModel(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            ActivateItem(IoC.Get<RssSourcesViewModel>());
            Items.Add(IoC.Get<RssEntriesViewModel>());

            using (var db = new RssContext())
            {
                _rssSources.AddRange(db.RssSources.ToList());
            }

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private bool _isBusy = false;
        private int _counter = 0;
        private int _seconds;

        private async void _timer_Tick(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            Seconds = 60 - _counter;
            _counter++;
            if (_counter <= 60)
                return;


            _isBusy = true;
            await Task.Run(() =>
            {
                foreach (var rssSource in _rssSources)
                {
                    using (var reader = XmlReader.Create(rssSource.Url))
                    {
                        var feed = SyndicationFeed.Load(reader);

                        foreach (var item in feed.Items)
                        {
                            var newRssEntry = new RssEntry
                            {
                                GeneratedId = item.Id,
                                Title = item.Title.Text,
                                Url = item.Links.Any() ? item.Links.First().Uri.ToString() : string.Empty
                            };

                            var isNew = SaveRss(newRssEntry);
                            if (!isNew)
                                continue;

                            _aggregator.PublishOnCurrentThread(new Events.ReadingRssEvent(newRssEntry));
                            ReadAloud(item.Title.Text);
                            _aggregator.PublishOnCurrentThread(new DoneReadingEvent());
                        }
                    }
                }
            });
            _counter = 0;
            _isBusy = false;
        }

        public int Seconds
        {
            get => _seconds;
            set => Set(ref _seconds, value);
        }

        private bool SaveRss(RssEntry newRssEntry)
        {
            using (var db = new RssContext())
            {
                if (db.RssEntries.FirstOrDefault(r => r.Url == newRssEntry.Url) != null)
                    return false;

                var rssEntry = db.RssEntries.Add(newRssEntry);
                db.SaveChanges();
                _aggregator.PublishOnCurrentThread(new Events.RssFeedAdded(rssEntry));
                return true;
            }
        }

        private void ReadAloud(string titleText)
        {
            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = -2;     // -10...10

                // Synchronous
                synthesizer.Speak(titleText);
            }
        }

        public void Handle(RssSourcesUpdatedEvent message)
        {
            _rssSources.Clear();
            using (var db = new RssContext())
            {
                _rssSources.AddRange(db.RssSources.ToList());
            }
        }
    }
}
