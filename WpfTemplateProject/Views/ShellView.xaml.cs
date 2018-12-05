using System.Diagnostics;
using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using RSSLoudReader.Events;
using RSSLoudReader.Models;

namespace RSSLoudReader.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : IHandle<Events.ReadingRssEvent>, IHandle<Events.DoneReadingEvent>
    {
        private RssEntry _lastRssEntry;

        public ShellView()
        {
            InitializeComponent();

            Loaded += ShellView_Loaded;
        }

        private void ShellView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IoC.Get<IEventAggregator>().Subscribe(this);
            TaskbarIcon.TrayBalloonTipClicked += TaskbarIcon_TrayBalloonTipClicked;
        }

        private void TaskbarIcon_TrayBalloonTipClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(_lastRssEntry.Url);
        }

        public void Handle(ReadingRssEvent message)
        {
            _lastRssEntry = message.RssEntry;
            TaskbarIcon.ShowBalloonTip(message.RssEntry.Title, 
                message.RssEntry.Url, BalloonIcon.Info);
        }

        public void Handle(DoneReadingEvent message)
        {
            TaskbarIcon.HideBalloonTip();
        }
    }
}
