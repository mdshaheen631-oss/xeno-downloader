using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using XenoDownloader.Models;

namespace XenoDownloader.Services
{
    public class DownloadManager
    {
        private readonly HttpClient _httpClient;
        private readonly SettingsService _settingsService;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokens = new();
        private readonly string _historyFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public ObservableCollection<DownloadItem> Downloads { get; } = new();

        public event Action<DownloadItem>? DownloadCompleted;
        public event Action<DownloadItem, string>? DownloadFailed;

        public DownloadManager(SettingsService settingsService)
        {
            _settingsService = settingsService;
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(60) // High timeout for large media
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) XenoDownloader/1.0");

            int maxConcurrent = Math.Clamp(_settingsService.CurrentSettings.MaxConcurrentDownloads, 1, 10);
            _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _historyFilePath = Path.Combine(appData, "XenoDownloader", "history.json");

            LoadHistory();
        }

        public DownloadItem EnqueueDownload(string url, string title, string destinationFolder, string? fileName = null, string quality = "Best", string? thumbnailUrl = null, long expectedSize = 0)
        {
            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                destinationFolder = _settingsService.CurrentSettings.DownloadDirectory;
            }

            Directory.CreateDirectory(destinationFolder);

            string safeFileName = string.IsNullOrWhiteSpace(fileName) ? MediaAnalyzerService.SanitizeFileName(title) + ".mp4" : fileName;
            string targetPath = Path.Combine(destinationFolder, safeFileName);

            // Avoid overwriting existing completed files
            int counter = 1;
            string baseName = Path.GetFileNameWithoutExtension(safeFileName);
            string ext = Path.GetExtension(safeFileName);
            while (File.Exists(targetPath) && Downloads.Any(d => d.DestinationFilePath == targetPath && d.Status == DownloadStatus.Completed))
            {
                targetPath = Path.Combine(destinationFolder, $"{baseName} ({counter}){ext}");
                counter++;
            }

            var item = new DownloadItem
            {
                Url = url,
                Title = title,
                ThumbnailUrl = thumbnailUrl,
                FileName = Path.GetFileName(targetPath),
                DestinationFilePath = targetPath,
                TotalBytes = expectedSize,
                Quality = quality,
                Status = DownloadStatus.Queued,
                CreatedAt = DateTime.UtcNow
            };

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Downloads.Insert(0, item);
            });

            _ = ProcessQueueAsync(item);
            SaveHistory();
            return item;
        }

        public void PauseDownload(DownloadItem item)
        {
            if (item.CanPause && _cancellationTokens.TryRemove(item.Id, out var cts))
            {
                cts.Cancel();
                item.Status = DownloadStatus.Paused;
                item.SpeedBytesPerSec = 0;
                item.EstimatedTimeRemaining = null;
                LoggerService.LogInfo($"Paused download: {item.Title}");
                SaveHistory();
            }
        }

        public void ResumeDownload(DownloadItem item)
        {
            if (item.CanResume)
            {
                item.Status = DownloadStatus.Queued;
                item.ErrorMessage = null;
                _ = ProcessQueueAsync(item);
                LoggerService.LogInfo($"Resumed download: {item.Title}");
                SaveHistory();
            }
        }

        public void CancelDownload(DownloadItem item)
        {
            if (_cancellationTokens.TryRemove(item.Id, out var cts))
            {
                cts.Cancel();
            }

            item.Status = DownloadStatus.Cancelled;
            item.SpeedBytesPerSec = 0;
            item.EstimatedTimeRemaining = null;
            LoggerService.LogInfo($"Cancelled download: {item.Title}");
            SaveHistory();
        }

        public void RetryDownload(DownloadItem item)
        {
            if (item.CanRetry)
            {
                item.Status = DownloadStatus.Queued;
                item.ErrorMessage = null;
                item.DownloadedBytes = 0;
                item.ProgressPercentage = 0;
                _ = ProcessQueueAsync(item);
                SaveHistory();
            }
        }

        public void RemoveDownload(DownloadItem item, bool deleteFile = false)
        {
            if (item.IsActive)
            {
                CancelDownload(item);
            }

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Downloads.Remove(item);
            });

            if (deleteFile && File.Exists(item.DestinationFilePath))
            {
                try
                {
                    File.Delete(item.DestinationFilePath);
                }
                catch (Exception ex)
                {
                    LoggerService.LogWarning($"Could not delete file {item.DestinationFilePath}: {ex.Message}");
                }
            }

            SaveHistory();
        }

        public void PauseAll()
        {
            foreach (var item in Downloads.Where(d => d.CanPause).ToList())
            {
                PauseDownload(item);
            }
        }

        public void ResumeAll()
        {
            foreach (var item in Downloads.Where(d => d.CanResume).ToList())
            {
                ResumeDownload(item);
            }
        }

        public void ClearCompleted()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var completed = Downloads.Where(d => d.Status == DownloadStatus.Completed || d.Status == DownloadStatus.Cancelled).ToList();
                foreach (var item in completed)
                {
                    Downloads.Remove(item);
                }
            });
            SaveHistory();
        }

        public void OpenDownloadedFile(DownloadItem item)
        {
            try
            {
                if (File.Exists(item.DestinationFilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = item.DestinationFilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show($"The file could not be found at:\n{item.DestinationFilePath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to open file: {item.DestinationFilePath}", ex);
            }
        }

        public void OpenContainingFolder(DownloadItem item)
        {
            try
            {
                string folder = Path.GetDirectoryName(item.DestinationFilePath) ?? _settingsService.CurrentSettings.DownloadDirectory;
                if (File.Exists(item.DestinationFilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{item.DestinationFilePath}\"");
                }
                else if (Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folder,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to open folder for: {item.DestinationFilePath}", ex);
            }
        }

        private async Task ProcessQueueAsync(DownloadItem item)
        {
            await _semaphore.WaitAsync();

            var cts = new CancellationTokenSource();
            _cancellationTokens[item.Id] = cts;

            try
            {
                if (cts.IsCancellationRequested || item.Status == DownloadStatus.Cancelled || item.Status == DownloadStatus.Paused)
                {
                    return;
                }

                await ExecuteDownloadAsync(item, cts.Token);
            }
            finally
            {
                _cancellationTokens.TryRemove(item.Id, out _);
                _semaphore.Release();
            }
        }

        private async Task ExecuteDownloadAsync(DownloadItem item, CancellationToken cancellationToken)
        {
            item.Status = DownloadStatus.Connecting;

            long existingLength = 0;
            if (File.Exists(item.DestinationFilePath))
            {
                var fi = new FileInfo(item.DestinationFilePath);
                existingLength = fi.Length;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, item.Url);

                // Try resume if partial file exists
                if (existingLength > 0)
                {
                    request.Headers.Range = new RangeHeaderValue(existingLength, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                bool isResuming = response.StatusCode == System.Net.HttpStatusCode.PartialContent;
                if (!response.IsSuccessStatusCode && !isResuming)
                {
                    throw new HttpRequestException($"Server returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                }

                long totalBytes = 0;
                if (isResuming && response.Content.Headers.ContentRange?.Length.HasValue == true)
                {
                    totalBytes = response.Content.Headers.ContentRange.Length.Value;
                }
                else if (response.Content.Headers.ContentLength.HasValue)
                {
                    totalBytes = response.Content.Headers.ContentLength.Value + (isResuming ? existingLength : 0);
                }
                else if (item.TotalBytes > 0)
                {
                    totalBytes = item.TotalBytes;
                }

                item.TotalBytes = totalBytes;
                item.DownloadedBytes = isResuming ? existingLength : 0;
                item.Status = DownloadStatus.Downloading;

                int bufferSize = Math.Max(16384, _settingsService.CurrentSettings.BufferSizeBytes);
                byte[] buffer = new byte[bufferSize];

                // Append or overwrite depending on resuming
                FileMode fileMode = isResuming ? FileMode.Append : FileMode.Create;

                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(item.DestinationFilePath, fileMode, FileAccess.Write, FileShare.Read, bufferSize, useAsync: true))
                {
                    var stopwatch = Stopwatch.StartNew();
                    long bytesSinceLastCalc = 0;
                    long lastReportTimeMs = 0;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                        if (bytesRead == 0) break;

                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                        item.DownloadedBytes += bytesRead;
                        bytesSinceLastCalc += bytesRead;

                        long elapsedMs = stopwatch.ElapsedMilliseconds;
                        // Throttle UI property changes to at most once per 250ms for buttery smooth, low-CPU rendering
                        if (elapsedMs - lastReportTimeMs >= 250)
                        {
                            double seconds = (elapsedMs - lastReportTimeMs) / 1000.0;
                            if (seconds > 0)
                            {
                                item.SpeedBytesPerSec = bytesSinceLastCalc / seconds;
                                if (item.SpeedBytesPerSec > 0 && item.TotalBytes > item.DownloadedBytes)
                                {
                                    double remainingBytes = item.TotalBytes - item.DownloadedBytes;
                                    item.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingBytes / item.SpeedBytesPerSec);
                                }
                            }

                            bytesSinceLastCalc = 0;
                            lastReportTimeMs = elapsedMs;
                        }
                    }
                }

                item.Status = DownloadStatus.Completed;
                item.ProgressPercentage = 100;
                item.SpeedBytesPerSec = 0;
                item.EstimatedTimeRemaining = TimeSpan.Zero;
                item.CompletedAt = DateTime.UtcNow;

                LoggerService.LogInfo($"Download completed: {item.Title} -> {item.DestinationFilePath}");
                DownloadCompleted?.Invoke(item);
                SaveHistory();
            }
            catch (OperationCanceledException)
            {
                if (item.Status != DownloadStatus.Cancelled)
                {
                    item.Status = DownloadStatus.Paused;
                }
                item.SpeedBytesPerSec = 0;
                item.EstimatedTimeRemaining = null;
            }
            catch (Exception ex)
            {
                item.Status = DownloadStatus.Failed;
                item.ErrorMessage = ex.Message;
                item.SpeedBytesPerSec = 0;
                item.EstimatedTimeRemaining = null;

                LoggerService.LogError($"Download failed for {item.Title}", ex);
                DownloadFailed?.Invoke(item, ex.Message);
                SaveHistory();
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string json = File.ReadAllText(_historyFilePath);
                    var items = JsonSerializer.Deserialize<List<DownloadItem>>(json);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            // Reset active statuses to paused on app launch
                            if (item.Status == DownloadStatus.Downloading || item.Status == DownloadStatus.Connecting || item.Status == DownloadStatus.Queued)
                            {
                                item.Status = DownloadStatus.Paused;
                            }
                            Downloads.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to load download history", ex);
            }
        }

        public void SaveHistory()
        {
            try
            {
                var directory = Path.GetDirectoryName(_historyFilePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                var items = Downloads.ToList();
                string json = JsonSerializer.Serialize(items, JsonOptions);
                File.WriteAllText(_historyFilePath, json);
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to save download history", ex);
            }
        }

        public void ClearHistory()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Downloads.Clear();
            });
            SaveHistory();
        }
    }
}
