using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using XenoDownloader.Services;
using XenoDownloader.ViewModels;

namespace XenoDownloader.Views
{
    public partial class BrowserView : UserControl
    {
        private bool _isInitialized;

        public BrowserView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;

            try
            {
                // Set custom user data folder in AppData for clean isolation
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string userDataFolder = Path.Combine(appData, "XenoDownloader", "WebView2Data");
                Directory.CreateDirectory(userDataFolder);

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await WebViewControl.EnsureCoreWebView2Async(env);

                WebViewControl.CoreWebView2.SourceChanged += OnSourceChanged;
                WebViewControl.CoreWebView2.NavigationStarting += OnNavigationStarting;
                WebViewControl.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

                string initialUrl = "https://www.youtube.com";
                TxtAddress.Text = initialUrl;
                WebViewControl.Source = new Uri(initialUrl);

                _isInitialized = true;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to initialize WebView2", ex);
                LoadingOverlay.Visibility = Visibility.Visible;
                TxtStatus.Text = "WebView2 runtime required for embedded browser.";
            }
        }

        private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            TxtStatus.Text = "Connecting...";
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            TxtStatus.Text = e.IsSuccess ? "Page Loaded" : "Navigation Error";
        }

        private void OnSourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (WebViewControl?.Source != null)
            {
                TxtAddress.Text = WebViewControl.Source.ToString();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (WebViewControl?.CanGoBack == true) WebViewControl.GoBack();
        }

        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            if (WebViewControl?.CanGoForward == true) WebViewControl.GoForward();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            WebViewControl?.Reload();
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo("https://www.youtube.com");
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(TxtAddress.Text);
        }

        private void TxtAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateTo(TxtAddress.Text);
            }
        }

        private void NavigateTo(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            string target = url.Trim();
            if (!target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // If contains dot, assume domain, else google search
                target = target.Contains('.') ? "https://" + target : $"https://www.google.com/search?q={Uri.EscapeDataString(target)}";
            }

            try
            {
                WebViewControl.Source = new Uri(target);
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Invalid navigation URI: {target}", ex);
            }
        }

        private void BtnDownloadCurrentPage_Click(object sender, RoutedEventArgs e)
        {
            string currentUrl = WebViewControl?.Source?.ToString() ?? TxtAddress.Text;
            if (!string.IsNullOrWhiteSpace(currentUrl) && DataContext is MainViewModel vm)
            {
                vm.SendUrlFromBrowserCommand.Execute(currentUrl);
            }
        }
    }
}
