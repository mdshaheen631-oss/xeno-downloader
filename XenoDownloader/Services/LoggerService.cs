namespace XenoDownloader.Services
{
    public static class LoggerService
    {
        private static readonly object _lock = new();
        private static readonly string _logDirectory;
        private static readonly string _logFile;

        static LoggerService()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                _logDirectory = Path.Combine(appData, "XenoDownloader", "Logs");
                Directory.CreateDirectory(_logDirectory);
                _logFile = Path.Combine(_logDirectory, $"xeno_{DateTime.UtcNow:yyyyMMdd}.log");
            }
            catch
            {
                _logDirectory = Path.GetTempPath();
                _logFile = Path.Combine(_logDirectory, "xeno_downloader.log");
            }
        }

        public static string LogFilePath => _logFile;

        public static void LogInfo(string message) => Write("INFO", message);
        public static void LogWarning(string message) => Write("WARN", message);
        public static void LogError(string message, Exception? ex = null)
        {
            string detail = ex != null ? $"{message} | Ex: {ex.GetType().Name}: {ex.Message}" : message;
            Write("ERROR", detail);
        }

        private static void Write(string level, string message)
        {
            // Sanitize against any potential token / secret leaks
            string sanitized = Sanitize(message);
            string line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {sanitized}";

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine);
                }
            }
            catch
            {
                // Silent fail for logging to never crash the app
            }
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Redact any patterns resembling tokens or passwords
            input = System.Text.RegularExpressions.Regex.Replace(input, @"(ghp_[a-zA-Z0-9]{36}|Bearer\s+[a-zA-Z0-9\._\-]+|password=[^&\s]+|token=[^&\s]+)", "[REDACTED]");
            return input;
        }
    }
}
