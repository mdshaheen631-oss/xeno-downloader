using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using XenoDownloader.Models;
using XenoDownloader.Services;

namespace XenoDownloader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _currentSection = "Home";
        private string _inputUrl = string.Empty;
        private bool _isAnalyzing;
        private string? _analysisError;
        private MediaMetadata? _currentMedia;
        private MediaFormatOption? _selectedFormat;
        private string _customDownloadFolder = string.Empty;
        private string _browserAddress = "https://www.youtube.com";
        private string _downloadFilter = "All";

        public SettingsService SettingsService { get; }
        public DownloadManager DownloadManager { get; }
        public MediaAnalyzerService AnalyzerService { get; }
        public ClipboardService ClipboardService { get; }

        public string CurrentSection
        {
            get => _currentSection;
            set => SetProperty(ref _currentSection, value);
        }

        public string InputUrl
        {
            get => _inputUrl;
            set
            {
                if (SetProperty(ref _inputUrl, value))
                {
                    AnalysisError = null;
                }
            }
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => SetProperty(ref _isAnalyzing, value);
        }

        public string? AnalysisError
        {
            get => _analysisError;
            set => SetProperty(ref _analysisError, value);
        }

        public MediaMetadata? CurrentMedia
        {
            get => _currentMedia;
            set => SetProperty(ref _currentMedia, value);
        }

        public MediaFormatOption? SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        public string CustomDownloadFolder
        {
            get => _customDownloadFolder;
            set => SetProperty(ref _customDownloadFolder, value);
        }

        public string BrowserAddress
        {
            get => _browserAddress;
            set => SetProperty(ref _browserAddress, value);
        }

        public string DownloadFilter
        {
            get => _downloadFilter;
            set
            {
                if (SetProperty(ref _downloadFilter, value))
                {
                    OnPropertyChanged(nameof(FilteredDownloads));
                }
            }
        }

        public ObservableCollection<DownloadItem> AllDownloads => DownloadManager.Downloads;

        public IEnumerable<DownloadItem> FilteredDownloads => DownloadFilter switch
        {
            "Active" => AllDownloads.Where(d => d.IsActive || d.Status == DownloadStatus.Queued),
            "Completed" => AllDownloads.Where(d => d.Status == DownloadStatus.Completed),
            "Paused" => AllDownloads.Where(d => d.Status == DownloadStatus.Paused),
            "Failed" => AllDownloads.Where(d => d.Status == DownloadStatus.Failed),
            _ => AllDownloads
        };

        public int ActiveDownloadsCount => AllDownloads.Count(d => d.IsActive);
        public int CompletedDownloadsCount => AllDownloads.Count(d => d.Status == DownloadStatus.Completed);
        public string TotalSpeedDisplay
        {
            get
            {
                double totalSpeed = AllDownloads.Where(d => d.IsActive).Sum(d => d.SpeedBytesPerSec);
                return totalSpeed > 0 ? $"{DownloadItem.FormatBytes((long)totalSpeed)}/s" : "0 B/s";
            }
        }

        // Navigation Commands
        public ICommand NavigateCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public ICommand PasteAndAnalyzeCommand { get; }
        public ICommand StartDownloadCommand { get; }
        public ICommand PauseDownloadCommand { get; }
        public ICommand ResumeDownloadCommand { get; }
        public ICommand CancelDownloadCommand { get; }
        public ICommand RetryDownloadCommand { get; }
        public ICommand RemoveDownloadCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand PauseAllCommand { get; }
        public ICommand ResumeAllCommand { get; }
        public ICommand ClearCompletedCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetSettingsCommand { get; }
        public ICommand SendUrlFromBrowserCommand { get; }
        public ICommand OpenGitHubCommand { get; }
        public ICommand OpenLogsCommand { get; }

        public MainViewModel()
        {
            SettingsService = new SettingsService();
            DownloadManager = new DownloadManager(SettingsService);
            AnalyzerService = new MediaAnalyzerService();
            ClipboardService = new ClipboardService();

            CustomDownloadFolder = SettingsService.CurrentSettings.DownloadDirectory;

            // Initialize Commands
            NavigateCommand = new RelayCommand(param =>
            {
                if (param is string section) CurrentSection = section;
            });

            AnalyzeCommand = new RelayCommand(async _ => await AnalyzeUrlAsync(InputUrl));
            PasteAndAnalyzeCommand = new RelayCommand(async _ =>
            {
                string text = ClipboardService.GetClipboardText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    InputUrl = text;
                    await AnalyzeUrlAsync(text);
                }
            });

            StartDownloadCommand = new RelayCommand(_ => ExecuteStartDownload());
            PauseDownloadCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.PauseDownload(item); });
            ResumeDownloadCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.ResumeDownload(item); });
            CancelDownloadCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.CancelDownload(item); });
            RetryDownloadCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.RetryDownload(item); });
            RemoveDownloadCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.RemoveDownload(item); });
            OpenFileCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.OpenDownloadedFile(item); });
            OpenFolderCommand = new RelayCommand(param => { if (param is DownloadItem item) DownloadManager.OpenContainingFolder(item); });

            PauseAllCommand = new RelayCommand(_ => DownloadManager.PauseAll());
            ResumeAllCommand = new RelayCommand(_ => DownloadManager.ResumeAll());
            ClearCompletedCommand = new RelayCommand(_ => DownloadManager.ClearCompleted());

            BrowseFolderCommand = new RelayCommand(_ => ChooseDownloadFolder());
            SaveSettingsCommand = new RelayCommand(_ =>
            {
                SettingsService.CurrentSettings.DownloadDirectory = CustomDownloadFolder;
                SettingsService.SaveSettings();
                MessageBox.Show("Settings saved successfully!", "Xeno Downloader", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            ResetSettingsCommand = new RelayCommand(_ =>
            {
                SettingsService.ResetSettings();
                CustomDownloadFolder = SettingsService.CurrentSettings.DownloadDirectory;
                OnPropertyChanged(nameof(SettingsService));
                MessageBox.Show("Settings reset to defaults.", "Xeno Downloader", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            SendUrlFromBrowserCommand = new RelayCommand(param =>
            {
                if (param is string url && !string.IsNullOrWhiteSpace(url))
                {
                    InputUrl = url;
                    CurrentSection = "Downloader";
                    _ = AnalyzeUrlAsync(url);
                }
            });

            OpenGitHubCommand = new RelayCommand(_ =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/mdshaheen631-oss/xeno-downloader",
                        UseShellExecute = true
                    });
                }
                catch { }
            });

            OpenLogsCommand = new RelayCommand(_ =>
            {
                try
                {
                    string path = LoggerService.LogFilePath;
                    if (File.Exists(path))
                    {
                        Process.Start("notepad.exe", path);
                    }
                }
                catch { }
            });

            // Hook download collection updates
            DownloadManager.Downloads.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(FilteredDownloads));
                OnPropertyChanged(nameof(ActiveDownloadsCount));
                OnPropertyChanged(nameof(CompletedDownloadsCount));
                OnPropertyChanged(nameof(TotalSpeedDisplay));
            };

            // Hook clipboard detection if configured
            ClipboardService.MediaUrlDetected += OnClipboardMediaDetected;
            ClipboardService.EnableDetection(SettingsService.CurrentSettings.AutoClipboardDetect);
        }

        private async Task AnalyzeUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                AnalysisError = "Please enter a valid media link or video URL.";
                return;
            }

            IsAnalyzing = true;
            AnalysisError = null;
            CurrentMedia = null;
            SelectedFormat = null;

            try
            {
                var result = await AnalyzerService.AnalyzeUrlAsync(url);
                CurrentMedia = result;
                SelectedFormat = result.Formats.FirstOrDefault();

                // 4K Video Downloader Smart Mode: If enabled, start download immediately with preset preferences
                if (SettingsService.CurrentSettings.SmartModeEnabled)
                {
                    ExecuteStartDownload();
                    CurrentSection = "Downloads";
                }
                else
                {
                    CurrentSection = "Downloader";
                }
            }
            catch (Exception ex)
            {
                AnalysisError = ex.Message;
                LoggerService.LogError("Analysis error in ViewModel", ex);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private void ExecuteStartDownload()
        {
            if (CurrentMedia == null) return;

            string targetUrl = SelectedFormat?.DirectDownloadUrl ?? CurrentMedia.OriginalUrl;
            string quality = SelectedFormat?.Label ?? "Best";
            string folder = !string.IsNullOrWhiteSpace(CustomDownloadFolder) ? CustomDownloadFolder : SettingsService.CurrentSettings.DownloadDirectory;
            long size = SelectedFormat?.EstimatedSize ?? CurrentMedia.TotalSizeBytes;

            DownloadManager.EnqueueDownload(
                url: targetUrl,
                title: CurrentMedia.Title,
                destinationFolder: folder,
                fileName: CurrentMedia.SuggestedFileName,
                quality: quality,
                thumbnailUrl: CurrentMedia.ThumbnailUrl,
                expectedSize: size
            );

            CurrentSection = "Downloads";
        }

        private void ChooseDownloadFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Destination Folder for Downloads",
                UseDescriptionForTitle = true,
                SelectedPath = CustomDownloadFolder
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CustomDownloadFolder = dialog.SelectedPath;
            }
        }

        private void OnClipboardMediaDetected(string url)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var answer = MessageBox.Show(
                    $"Media URL detected on clipboard:\n\n{url}\n\nDo you want to analyze and download it with Xeno Downloader?",
                    "Xeno Downloader - Clipboard Detection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    InputUrl = url;
                    CurrentSection = "Downloader";
                    _ = AnalyzeUrlAsync(url);
                }
            });
        }
    }
}
