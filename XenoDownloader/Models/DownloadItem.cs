using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace XenoDownloader.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        private DownloadStatus _status = DownloadStatus.Queued;
        private long _downloadedBytes;
        private long _totalBytes;
        private double _progressPercentage;
        private double _speedBytesPerSec;
        private TimeSpan? _estimatedTimeRemaining;
        private string? _errorMessage;
        private string _destinationFilePath = string.Empty;
        private string _fileName = string.Empty;
        private string _title = string.Empty;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; } = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string? ThumbnailUrl { get; set; }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string DestinationFilePath
        {
            get => _destinationFilePath;
            set => SetProperty(ref _destinationFilePath, value);
        }

        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                if (SetProperty(ref _totalBytes, value))
                {
                    OnPropertyChanged(nameof(TotalSizeDisplay));
                    UpdateProgress();
                }
            }
        }

        public long DownloadedBytes
        {
            get => _downloadedBytes;
            set
            {
                if (SetProperty(ref _downloadedBytes, value))
                {
                    OnPropertyChanged(nameof(DownloadedSizeDisplay));
                    UpdateProgress();
                }
            }
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        public double SpeedBytesPerSec
        {
            get => _speedBytesPerSec;
            set
            {
                if (SetProperty(ref _speedBytesPerSec, value))
                {
                    OnPropertyChanged(nameof(SpeedDisplay));
                }
            }
        }

        public TimeSpan? EstimatedTimeRemaining
        {
            get => _estimatedTimeRemaining;
            set
            {
                if (SetProperty(ref _estimatedTimeRemaining, value))
                {
                    OnPropertyChanged(nameof(EtaDisplay));
                }
            }
        }

        public DownloadStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusDisplay));
                    OnPropertyChanged(nameof(CanPause));
                    OnPropertyChanged(nameof(CanResume));
                    OnPropertyChanged(nameof(CanCancel));
                    OnPropertyChanged(nameof(CanRetry));
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string Quality { get; set; } = "Best Available";
        public string Format { get; set; } = "MP4";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Computed display properties
        [JsonIgnore]
        public string StatusDisplay => Status switch
        {
            DownloadStatus.Queued => "In Queue",
            DownloadStatus.Connecting => "Connecting...",
            DownloadStatus.Downloading => "Downloading",
            DownloadStatus.Paused => "Paused",
            DownloadStatus.Completed => "Completed",
            DownloadStatus.Failed => "Failed",
            DownloadStatus.Cancelled => "Cancelled",
            _ => Status.ToString()
        };

        [JsonIgnore]
        public string DownloadedSizeDisplay => FormatBytes(DownloadedBytes);

        [JsonIgnore]
        public string TotalSizeDisplay => TotalBytes > 0 ? FormatBytes(TotalBytes) : "Unknown size";

        [JsonIgnore]
        public string SpeedDisplay => SpeedBytesPerSec > 0 ? $"{FormatBytes((long)SpeedBytesPerSec)}/s" : "--";

        [JsonIgnore]
        public string EtaDisplay
        {
            get
            {
                if (!EstimatedTimeRemaining.HasValue || Status != DownloadStatus.Downloading)
                    return "--";
                var t = EstimatedTimeRemaining.Value;
                return t.TotalHours >= 1 ? $"{(int)t.TotalHours}h {t.Minutes}m" : $"{t.Minutes:D2}:{t.Seconds:D2}";
            }
        }

        [JsonIgnore]
        public bool CanPause => Status == DownloadStatus.Downloading || Status == DownloadStatus.Connecting;

        [JsonIgnore]
        public bool CanResume => Status == DownloadStatus.Paused;

        [JsonIgnore]
        public bool CanCancel => Status == DownloadStatus.Downloading || Status == DownloadStatus.Connecting || Status == DownloadStatus.Queued || Status == DownloadStatus.Paused;

        [JsonIgnore]
        public bool CanRetry => Status == DownloadStatus.Failed || Status == DownloadStatus.Cancelled;

        [JsonIgnore]
        public bool IsActive => Status == DownloadStatus.Downloading || Status == DownloadStatus.Connecting;

        [JsonIgnore]
        public bool IsCompleted => Status == DownloadStatus.Completed;

        private void UpdateProgress()
        {
            if (TotalBytes > 0)
            {
                ProgressPercentage = Math.Clamp(Math.Round(((double)DownloadedBytes / TotalBytes) * 100.0, 1), 0, 100);
            }
            else if (Status == DownloadStatus.Completed)
            {
                ProgressPercentage = 100;
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {suffixes[order]}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
