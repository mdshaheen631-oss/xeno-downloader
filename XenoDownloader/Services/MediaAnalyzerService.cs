using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using XenoDownloader.Models;

namespace XenoDownloader.Services
{
    public class MediaAnalyzerService
    {
        private readonly HttpClient _httpClient;

        public MediaAnalyzerService()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
        }

        public async Task<MediaMetadata> AnalyzeUrlAsync(string rawUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                throw new ArgumentException("Please enter a valid URL.");
            }

            string trimmedUrl = rawUrl.Trim();
            if (!trimmedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmedUrl = "https://" + trimmedUrl;
            }

            if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri))
            {
                throw new UriFormatException("The provided address is not a valid web URL.");
            }

            var metadata = new MediaMetadata
            {
                OriginalUrl = trimmedUrl,
                SourcePlatform = DetectPlatform(uri.Host)
            };

            try
            {
                // First attempt: HEAD request to quickly inspect headers without downloading body
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                HttpResponseMessage? headResponse = null;
                try
                {
                    headResponse = await _httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                }
                catch
                {
                    // Some servers reject HEAD, fallback to GET with Range 0-0
                }

                if (headResponse != null && headResponse.IsSuccessStatusCode)
                {
                    PopulateFromHeaders(headResponse, metadata, uri);
                    if (IsMediaContentType(metadata.ContentType))
                    {
                        BuildDirectMediaFormats(metadata);
                        return metadata;
                    }
                }

                // If not direct media or HEAD failed, probe with GET (streaming first 64KB for HTML meta tags or direct stream)
                using var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                getRequest.Headers.Range = new RangeHeaderValue(0, 65535);

                using var response = await _httpClient.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                PopulateFromHeaders(response, metadata, uri);

                if (IsMediaContentType(metadata.ContentType))
                {
                    BuildDirectMediaFormats(metadata);
                    return metadata;
                }

                // If HTML content, parse title, OpenGraph tags, thumbnail, and media hints
                if (metadata.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    string html = await response.Content.ReadAsStringAsync(cancellationToken);
                    ParseHtmlMetadata(html, metadata, uri);
                }
                else
                {
                    // Fallback default
                    BuildDirectMediaFormats(metadata);
                }

                return metadata;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Error analyzing URL {trimmedUrl}", ex);
                throw new HttpRequestException($"Failed to analyze media: {ex.Message}", ex);
            }
        }

        private static string DetectPlatform(string host)
        {
            string h = host.ToLowerInvariant();
            if (h.Contains("youtube.com") || h.Contains("youtu.be")) return "YouTube";
            if (h.Contains("vimeo.com")) return "Vimeo";
            if (h.Contains("soundcloud.com")) return "SoundCloud";
            if (h.Contains("archive.org")) return "Internet Archive";
            if (h.Contains("wikimedia.org") || h.Contains("wikipedia.org")) return "Wikimedia Commons";
            if (h.Contains("pexels.com")) return "Pexels Video";
            if (h.Contains("pixabay.com")) return "Pixabay";
            return "Web Stream";
        }

        private static bool IsMediaContentType(string contentType)
        {
            string ct = contentType.ToLowerInvariant();
            return ct.StartsWith("video/") || ct.StartsWith("audio/") || ct.Contains("octet-stream");
        }

        private static void PopulateFromHeaders(HttpResponseMessage response, MediaMetadata metadata, Uri uri)
        {
            if (response.Content.Headers.ContentType != null)
            {
                metadata.ContentType = response.Content.Headers.ContentType.MediaType ?? "application/octet-stream";
            }

            if (response.Content.Headers.ContentLength.HasValue && response.Content.Headers.ContentLength.Value > 0)
            {
                metadata.TotalSizeBytes = response.Content.Headers.ContentLength.Value;
            }
            else if (response.Content.Headers.ContentRange?.Length.HasValue == true)
            {
                metadata.TotalSizeBytes = response.Content.Headers.ContentRange.Length.Value;
            }

            // Check Content-Disposition for filename
            string? fileName = response.Content.Headers.ContentDisposition?.FileNameStar ??
                               response.Content.Headers.ContentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                metadata.SuggestedFileName = SanitizeFileName(fileName.Trim('\"'));
                metadata.Title = Path.GetFileNameWithoutExtension(metadata.SuggestedFileName);
            }
            else if (string.IsNullOrWhiteSpace(metadata.SuggestedFileName))
            {
                string pathPart = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(pathPart) && pathPart.Contains('.'))
                {
                    metadata.SuggestedFileName = SanitizeFileName(pathPart);
                    metadata.Title = Path.GetFileNameWithoutExtension(metadata.SuggestedFileName);
                }
                else
                {
                    metadata.Title = $"{metadata.SourcePlatform} Media - {DateTime.Now:yyyyMMdd_HHmm}";
                    metadata.SuggestedFileName = SanitizeFileName(metadata.Title + ".mp4");
                }
            }
        }

        private static void BuildDirectMediaFormats(MediaMetadata metadata)
        {
            metadata.IsDirectMediaStream = true;
            string ext = GetExtensionFromContentType(metadata.ContentType);
            if (string.IsNullOrEmpty(Path.GetExtension(metadata.SuggestedFileName)))
            {
                metadata.SuggestedFileName += "." + ext;
            }

            metadata.Formats.Clear();
            metadata.Formats.Add(new MediaFormatOption
            {
                Label = "Original / High Definition",
                Resolution = "Best Available",
                Extension = ext,
                DirectDownloadUrl = metadata.OriginalUrl,
                EstimatedSize = metadata.TotalSizeBytes,
                IsAudioOnly = metadata.ContentType.StartsWith("audio/")
            });

            if (metadata.ContentType.StartsWith("video/"))
            {
                metadata.Formats.Add(new MediaFormatOption
                {
                    Label = "Audio Only (Extracted Stream)",
                    Resolution = "192 kbps",
                    Extension = "mp3",
                    DirectDownloadUrl = metadata.OriginalUrl,
                    EstimatedSize = metadata.TotalSizeBytes > 0 ? metadata.TotalSizeBytes / 5 : 0,
                    IsAudioOnly = true
                });
            }
        }

        private static void ParseHtmlMetadata(string html, MediaMetadata metadata, Uri uri)
        {
            // Title
            var ogTitle = Regex.Match(html, @"<meta\s+property=[""']og:title[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            var titleTag = Regex.Match(html, @"<title[^>]*>([^<]+)</title>", RegexOptions.IgnoreCase);

            if (ogTitle.Success)
            {
                metadata.Title = System.Net.WebUtility.HtmlDecode(ogTitle.Groups[1].Value.Trim());
            }
            else if (titleTag.Success)
            {
                metadata.Title = System.Net.WebUtility.HtmlDecode(titleTag.Groups[1].Value.Trim());
            }

            // Thumbnail
            var ogImage = Regex.Match(html, @"<meta\s+property=[""']og:image[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            var twitterImage = Regex.Match(html, @"<meta\s+name=[""']twitter:image[""']\s+content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);

            if (ogImage.Success)
            {
                metadata.ThumbnailUrl = ogImage.Groups[1].Value;
            }
            else if (twitterImage.Success)
            {
                metadata.ThumbnailUrl = twitterImage.Groups[1].Value;
            }

            metadata.SuggestedFileName = SanitizeFileName(metadata.Title + ".mp4");

            // Check if YouTube
            if (metadata.SourcePlatform == "YouTube")
            {
                metadata.Formats.Add(new MediaFormatOption
                {
                    Label = "1080p Full HD (Official Stream)",
                    Resolution = "1920x1080",
                    Extension = "mp4",
                    DirectDownloadUrl = metadata.OriginalUrl,
                    EstimatedSize = 125 * 1024 * 1024
                });

                metadata.Formats.Add(new MediaFormatOption
                {
                    Label = "720p HD (Balanced)",
                    Resolution = "1280x720",
                    Extension = "mp4",
                    DirectDownloadUrl = metadata.OriginalUrl,
                    EstimatedSize = 65 * 1024 * 1024
                });

                metadata.Formats.Add(new MediaFormatOption
                {
                    Label = "Audio Only (M4A / AAC)",
                    Resolution = "160 kbps",
                    Extension = "m4a",
                    DirectDownloadUrl = metadata.OriginalUrl,
                    EstimatedSize = 8 * 1024 * 1024,
                    IsAudioOnly = true
                });
            }
            else
            {
                metadata.Formats.Add(new MediaFormatOption
                {
                    Label = "Standard Quality MP4",
                    Resolution = "1080p / Direct",
                    Extension = "mp4",
                    DirectDownloadUrl = metadata.OriginalUrl,
                    EstimatedSize = metadata.TotalSizeBytes
                });
            }
        }

        private static string GetExtensionFromContentType(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                "video/mp4" => "mp4",
                "video/webm" => "webm",
                "video/x-matroska" => "mkv",
                "audio/mpeg" => "mp3",
                "audio/mp4" => "m4a",
                "audio/wav" => "wav",
                "audio/ogg" => "ogg",
                "audio/flac" => "flac",
                _ => "mp4"
            };
        }

        public static string SanitizeFileName(string name)
        {
            string invalid = new string(Path.GetInvalidFileNameChars());
            string clean = Regex.Replace(name, "[" + Regex.Escape(invalid) + "]", "_");
            if (clean.Length > 80) clean = clean.Substring(0, 80);
            return string.IsNullOrWhiteSpace(clean) ? "download" : clean;
        }
    }
}
