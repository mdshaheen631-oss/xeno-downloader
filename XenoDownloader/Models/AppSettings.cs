namespace XenoDownloader.Models
{
    public class AppSettings
    {
        public string DownloadDirectory { get; set; } = string.Empty;
        public int MaxConcurrentDownloads { get; set; } = 3;
        public bool AutoClipboardDetect { get; set; } = false;
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool CloseToTray { get; set; } = false;
        public string Theme { get; set; } = "Dark";
        public bool ShowNotifications { get; set; } = true;
        public bool SoundAlertOnComplete { get; set; } = false;
        public int BufferSizeBytes { get; set; } = 65536; // 64 KB streaming buffer for optimal performance

        public AppSettings()
        {
            try
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                DownloadDirectory = Path.Combine(userProfile, "Downloads", "Xeno Downloader");
            }
            catch
            {
                DownloadDirectory = "C:\\Downloads\\Xeno Downloader";
            }
        }
    }
}
