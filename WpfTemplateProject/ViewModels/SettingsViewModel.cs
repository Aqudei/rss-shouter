using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSLoudReader.ViewModels
{
    sealed class SettingsViewModel : Screen
    {
        private int _delayIntervalSeconds;
        private readonly IDialogCoordinator _dialogCoordinator;

        public int DelayIntervalSeconds
        {
            get { return _delayIntervalSeconds; }
            set { Set(ref _delayIntervalSeconds, value); }
        }

        public SettingsViewModel(IDialogCoordinator dialogCoordinator)
        {
            DisplayName = "Settings";
            _dialogCoordinator = dialogCoordinator;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            DelayIntervalSeconds = Properties.Settings.Default.DELAY_INTERVAL_SECONDS;
        }

        public IEnumerable<IResult> Save()
        {
            yield return Task.Run(async () =>
              {
                  Properties.Settings.Default.DELAY_INTERVAL_SECONDS = DelayIntervalSeconds;
                  await _dialogCoordinator.ShowMessageAsync(this, "Success", "Settings Saved");
              }).AsResult();

        }
    }
}
