﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using NLog;
using NLog.Fluent;
using RSSLoudReader.Events;
using RSSLoudReader.Models;
using static System.Net.Mime.MediaTypeNames;
using LogManager = Caliburn.Micro.LogManager;

namespace RSSLoudReader.ViewModels
{
    sealed class ShellViewModel : Conductor<object>.Collection.OneActive, IHandle<Events.RssSourcesUpdatedEvent>
    {
        private readonly IEventAggregator _aggregator;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly List<RssSource> _rssSources = new List<RssSource>();
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public string MyImageSource { get => _myImageSource; set => Set(ref _myImageSource, value); }
        public ShellViewModel(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            ActivateItem(IoC.Get<RssSourcesViewModel>());
            Items.Add(IoC.Get<RssEntriesViewModel>());
            Items.Add(IoC.Get<SettingsViewModel>());

            using (var db = new RssContext())
            {
                _rssSources.AddRange(db.RssSources.ToList());
            }

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _aggregator.Subscribe(this);

            if (File.Exists(Properties.Settings.Default.LOGO))
            {
                MyImageSource = Properties.Settings.Default.LOGO;
            }
            else
            {
                MyImageSource = "/RSSLoudReader;component/Resources/Logo_ZENData_new.gif";
            }
        }

        private bool _isBusy = false;
        private int _counter = 0;
        private int _seconds;
        private volatile bool _cancelled;

        private string _myImageSource;

        public void Cancel()
        {
            _cancelled = true;
        }


        private async void _timer_Tick(object sender, EventArgs e)
        {
            if (_isBusy)
                return;

            Seconds = Properties.Settings.Default.DELAY_INTERVAL_SECONDS - _counter;
            _counter++;
            if (_counter <= Properties.Settings.Default.DELAY_INTERVAL_SECONDS)
                return;

            _isBusy = true;
            _cancelled = false;
            Message = "";
            await Task.Run(() =>
            {
                try
                {
                    foreach (var rssSource in _rssSources)
                    {
                        XmlReader reader;

                        Debug.WriteLine($"Procesing {rssSource.Url}...");

                        try
                        {
                            reader = XmlReader.Create(rssSource.Url);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Unable to process feeds from {rssSource.Url}");
                            Debug.WriteLine(ex.Message);

                            Execute.OnUIThread(() =>
                            {
                                Message += $"Unble to process feeds from {rssSource.Url}\n";
                                Message += $"Reason: {ex.Message}\n";
                            });

                            continue;
                        }

                        using (reader)
                        {

                            var feed = SyndicationFeed.Load(reader);

                            foreach (var item in feed.Items)
                            {
                                var newRssEntry = new RssEntry
                                {
                                    PublishedDate = item.PublishDate.DateTime > DateTime.MinValue ? item.PublishDate.DateTime : DateTime.Now,
                                    GeneratedId = item.Id,
                                    Title = item.Title.Text,
                                    Url = item.Links.Any() ? item.Links.First().Uri.ToString() : string.Empty
                                };

                                var isNew = SaveRss(newRssEntry);

                                if (!isNew)
                                    continue;

                                if (_cancelled)
                                    continue;

                                _aggregator.PublishOnCurrentThread(new Events.ReadingRssEvent(newRssEntry));
                                ReadAloud(item.Title.Text);
                                _aggregator.PublishOnCurrentThread(new DoneReadingEvent());
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    _logger.Error("Error at Rss Reader Loop.\n" + exception.Message);
                }

            });
            _counter = 0;
            _isBusy = false;
        }
        private string _message;

        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
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
