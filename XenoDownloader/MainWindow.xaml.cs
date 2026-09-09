using System.ComponentModel;
using System.Windows;
using XenoDownloader.Services;
using XenoDownloader.ViewModels;

namespace XenoDownloader
{
    public partial class MainWindow : Window
    {
        private TrayService? _trayService;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            StateChanged += OnStateChanged;
            Closing += OnClosing;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                _trayService = new TrayService(this, vm.DownloadManager, vm.SettingsService);
            }
        }

        private void OnStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && DataContext is MainViewModel vm && vm.SettingsService.CurrentSettings.MinimizeToTray)
            {
                _trayService?.MinimizeToTray();
            }
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SettingsService.CurrentSettings.CloseToTray)
            {
                e.Cancel = true;
                _trayService?.MinimizeToTray();
                return;
            }

            _trayService?.Dispose();
        }
    }
}
