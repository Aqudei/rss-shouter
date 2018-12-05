using System;
using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;
using RSSLoudReader.Models;

namespace RSSLoudReader.ViewModels
{
    sealed class RssSourcesViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        public BindableCollection<RssSource> RssSources { get; set; }
            = new BindableCollection<RssSource>();

        private string _name;
        private string _url;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string Url
        {
            get => _url;
            set => Set(ref _url, value);
        }

        public void Save()
        {
            using (var db = new RssContext())
            {
                if (db.RssSources.ToList().Any(r => r.Url == Url))
                    return;

                var rssSource = db.RssSources.Add(new RssSource
                {
                    Url = Url,
                    Name = Name
                });

                db.SaveChanges();
                RssSources.Add(rssSource);
                _eventAggregator.PublishOnCurrentThread(new Events.RssSourcesUpdatedEvent());
            }
        }

        public void Delete(RssSource rssSource)
        {
            try
            {
                using (var db = new RssContext())
                {
                    db.RssSources.Remove(db.RssSources.Find(rssSource.Id) ?? throw new InvalidOperationException());
                    db.SaveChanges();
                    RssSources.Remove(rssSource);
                    _eventAggregator.PublishOnCurrentThread(new Events.RssSourcesUpdatedEvent());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public RssSourcesViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DisplayName = "RSS Sources";

            using (var db = new RssContext())
            {
                RssSources.AddRange(db.RssSources.ToList());
            }
        }
    }
}
