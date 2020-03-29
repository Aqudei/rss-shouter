using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSLoudReader.ViewModels
{
    sealed class SettingsViewModel : Screen
    {
        private int _delayIntervalSeconds;
        private string logo;
        private readonly IDialogCoordinator _dialogCoordinator;
        public string Logo { get => logo; set => Set(ref logo, value); }
        public int DelayIntervalSeconds
        {
            get { return _delayIntervalSeconds; }
            set { Set(ref _delayIntervalSeconds, value); }
        }


        public void BrowseLogo()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {

            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Logo = dialog.FileName;
            }
        }

        public IEnumerable<IResult> ApplyLogo()
        {
            yield return Task.Run(async () =>
            {
                if (!string.IsNullOrWhiteSpace(Logo) && File.Exists(Logo))
                {
                    var logoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetFileName(Logo));
                    File.Copy(Logo, logoPath, true);
                    Properties.Settings.Default.LOGO = logoPath;
                    Properties.Settings.Default.Save();
                    await _dialogCoordinator.ShowMessageAsync(this, "Attention", "App needs to be restarted for chamges to take effect");
                }
            }).AsResult();

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
