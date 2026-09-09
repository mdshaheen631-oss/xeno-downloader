namespace XenoDownloader.Models
{
    public enum DownloadStatus
    {
        Queued,
        Connecting,
        Downloading,
        Paused,
        Completed,
        Failed,
        Cancelled
    }
}
