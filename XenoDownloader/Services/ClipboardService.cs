using System.Windows;
using System.Windows.Threading;

namespace XenoDownloader.Services
{
    public class ClipboardService
    {
        private readonly DispatcherTimer _timer;
        private string _lastClipboardText = string.Empty;
        public event Action<string>? MediaUrlDetected;

        public ClipboardService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.0) // Extremely lightweight 2-second interval, zero CPU impact
            };
            _timer.Tick += OnTimerTick;
        }

        public void EnableDetection(bool enable)
        {
            if (enable)
            {
                if (!_timer.IsEnabled) _timer.Start();
            }
            else
            {
                if (_timer.IsEnabled) _timer.Stop();
            }
        }

        public string GetClipboardText()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    return Clipboard.GetText().Trim();
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogWarning($"Failed to read clipboard: {ex.Message}");
            }
            return string.Empty;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            try
            {
                if (!Clipboard.ContainsText()) return;

                string currentText = Clipboard.GetText().Trim();
                if (string.IsNullOrWhiteSpace(currentText) || currentText == _lastClipboardText) return;

                _lastClipboardText = currentText;

                if (IsPotentialMediaUrl(currentText))
                {
                    MediaUrlDetected?.Invoke(currentText);
                }
            }
            catch
            {
                // Clipboard can sometimes be locked by other apps, ignore safely
            }
        }

        public static bool IsPotentialMediaUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string lower = text.ToLowerInvariant();
            return lower.Contains("youtube.com") || lower.Contains("youtu.be") ||
                   lower.Contains("vimeo.com") || lower.Contains("soundcloud.com") ||
                   lower.Contains("archive.org") || lower.Contains("pexels.com") ||
                   lower.Contains(".mp4") || lower.Contains(".webm") ||
                   lower.Contains(".mp3") || lower.Contains(".mkv") || lower.Contains(".m4a");
        }
    }
}
