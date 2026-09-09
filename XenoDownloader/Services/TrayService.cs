using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace XenoDownloader.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private readonly Window _mainWindow;
        private readonly DownloadManager _downloadManager;
        private readonly SettingsService _settingsService;

        public TrayService(Window mainWindow, DownloadManager downloadManager, SettingsService settingsService)
        {
            _mainWindow = mainWindow;
            _downloadManager = downloadManager;
            _settingsService = settingsService;

            InitializeTray();
        }

        private void InitializeTray()
        {
            try
            {
                _notifyIcon = new NotifyIcon
                {
                    Text = "Xeno Downloader",
                    Visible = true
                };

                // Load icon from resource or extract default
                try
                {
                    var iconUri = new Uri("pack://application:,,,/Resources/AppIcon.ico", UriKind.RelativeOrAbsolute);
                    var streamInfo = Application.GetResourceStream(iconUri);
                    if (streamInfo != null)
                    {
                        using var s = streamInfo.Stream;
                        _notifyIcon.Icon = new Icon(s);
                    }
                    else
                    {
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                }
                catch
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }

                _notifyIcon.DoubleClick += (s, e) => RestoreWindow();

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Open Xeno Downloader", null, (s, e) => RestoreWindow());
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("Pause All Downloads", null, (s, e) => _downloadManager.PauseAll());
                contextMenu.Items.Add("Resume All Downloads", null, (s, e) => _downloadManager.ResumeAll());
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

                _notifyIcon.ContextMenuStrip = contextMenu;

                // Subscribe to download notifications
                _downloadManager.DownloadCompleted += OnDownloadCompleted;
                _downloadManager.DownloadFailed += OnDownloadFailed;
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to initialize system tray icon", ex);
            }
        }

        public void MinimizeToTray()
        {
            _mainWindow.Hide();
            if (_settingsService.CurrentSettings.ShowNotifications)
            {
                ShowNotification("Xeno Downloader", "App is running in the system tray. Downloads continue in background.", ToolTipIcon.Info);
            }
        }

        public void RestoreWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void OnDownloadCompleted(Models.DownloadItem item)
        {
            if (_settingsService.CurrentSettings.ShowNotifications)
            {
                ShowNotification("Download Completed", $"\"{item.Title}\" has finished downloading.", ToolTipIcon.Info);
            }
        }

        private void OnDownloadFailed(Models.DownloadItem item, string error)
        {
            if (_settingsService.CurrentSettings.ShowNotifications)
            {
                ShowNotification("Download Failed", $"Failed to download \"{item.Title}\": {error}", ToolTipIcon.Error);
            }
        }

        public void ShowNotification(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            try
            {
                _notifyIcon?.ShowBalloonTip(3000, title, text, icon);
            }
            catch
            {
                // Ignore balloon tip errors
            }
        }

        private void ExitApplication()
        {
            _notifyIcon?.Dispose();
            Application.Current.Shutdown();
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}
