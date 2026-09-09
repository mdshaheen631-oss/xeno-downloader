using System.Windows;
using System.Windows.Threading;
using XenoDownloader.Services;

namespace XenoDownloader
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handlers to prevent unexpected crashes
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            LoggerService.LogInfo("Xeno Downloader application started successfully.");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LoggerService.LogError("Unhandled UI dispatcher exception", e.Exception);
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}\n\nThe application will continue running safely.",
                            "Xeno Downloader", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true; // Never crash the application
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LoggerService.LogError("Unhandled AppDomain exception", ex);
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LoggerService.LogError("Unobserved task exception", e.Exception);
            e.SetObserved(); // Prevent background tasks from tearing down process
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggerService.LogInfo("Xeno Downloader application exiting cleanly.");
            base.OnExit(e);
        }
    }
}
