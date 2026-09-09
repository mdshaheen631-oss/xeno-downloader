namespace XenoDownloader.Models
{
    public class MediaFormatOption
    {
        public string Label { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string Extension { get; set; } = "mp4";
        public string DirectDownloadUrl { get; set; } = string.Empty;
        public long EstimatedSize { get; set; }
        public bool IsAudioOnly { get; set; }
        public string DisplayText => $"{Label} ({Extension.ToUpperInvariant()}) - {(EstimatedSize > 0 ? DownloadItem.FormatBytes(EstimatedSize) : "Direct Stream")}";
    }

    public class MediaMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string Duration { get; set; } = "--:--";
        public string SuggestedFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "video/mp4";
        public long TotalSizeBytes { get; set; }
        public string SourcePlatform { get; set; } = "Direct / Web";
        public bool IsDirectMediaStream { get; set; } = true;
        public List<MediaFormatOption> Formats { get; set; } = new();
    }
}
