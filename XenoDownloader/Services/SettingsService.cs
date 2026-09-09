using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using XenoDownloader.Models;

namespace XenoDownloader.Services
{
    public class SettingsService
    {
        private static readonly string SettingsFolder;
        private static readonly string SettingsFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public AppSettings CurrentSettings { get; private set; }

        static SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SettingsFolder = Path.Combine(appData, "XenoDownloader");
            SettingsFilePath = Path.Combine(SettingsFolder, "settings.json");
        }

        public SettingsService()
        {
            CurrentSettings = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        if (string.IsNullOrWhiteSpace(settings.DownloadDirectory))
                        {
                            settings.DownloadDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Xeno Downloader");
                        }
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to load settings, using defaults", ex);
            }

            var defaultSettings = new AppSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        public void SaveSettings(AppSettings? settings = null)
        {
            try
            {
                if (settings != null) CurrentSettings = settings;
                Directory.CreateDirectory(SettingsFolder);

                string json = JsonSerializer.Serialize(CurrentSettings, JsonOptions);
                File.WriteAllText(SettingsFilePath, json);

                ApplyWindowsStartup(CurrentSettings.StartWithWindows);
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to save settings", ex);
            }
        }

        public void ResetSettings()
        {
            CurrentSettings = new AppSettings();
            SaveSettings(CurrentSettings);
        }

        private void ApplyWindowsStartup(bool startWithWindows)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                string appPath = Environment.ProcessPath ?? string.Empty;
                if (string.IsNullOrEmpty(appPath)) return;

                if (startWithWindows)
                {
                    key.SetValue("XenoDownloader", $"\"{appPath}\" --minimized");
                }
                else
                {
                    key.DeleteValue("XenoDownloader", false);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogWarning($"Could not update registry for Windows startup: {ex.Message}");
            }
        }
    }
}
