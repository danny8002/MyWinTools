

using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace web_tool_youtube_downloader
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("YouTube Video/Playlist Downloader");
            Console.WriteLine("=================================");
            Console.Write("Enter YouTube URL: ");
            
            string? url = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("Error: URL cannot be empty.");
                return;
            }

            Console.Write("Enter download folder path (leave empty for default Downloads folder): ");
            string? downloadPath = Console.ReadLine();
            
            // Use null if empty, otherwise use the provided path
            string? finalDownloadPath = string.IsNullOrWhiteSpace(downloadPath) ? null : downloadPath.Trim();

            try
            {
                await ProcessYouTubeUrlAsync(url, finalDownloadPath);
            }
            finally
            {
                // Cleanup resources
                await YDDownClient.Cleanup();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static async Task ProcessYouTubeUrlAsync(string url, string? downloadPath)
        {
            var options = new BrowserTypeLaunchOptions
            {
                Headless = false,
                ExecutablePath = "C:/Program Files/Google/Chrome/Application/chrome.exe",
                Args = new[] { "--user-data-dir=C:/web-tool-youtube-downloader" },
                DownloadsPath = downloadPath // Set custom download folder (null for default)
            };
            
            if (!string.IsNullOrEmpty(downloadPath))
            {
                Console.WriteLine($"Downloads will be saved to: {downloadPath}");
            }
            else
            {
                Console.WriteLine("Downloads will be saved to default Downloads folder");
            }
            
            await using var webClient = new YoutubeWebClient();
            
            // Initialize the browser for URL extraction
            await webClient.Init(options);
            
            // Extract URLs (handles both playlist and single video)
            var urls = await webClient.ExtractURLs(url);
            
            if (urls.Count == 0)
            {
                Console.WriteLine("No URLs found.");
                return;
            }
            
            Console.WriteLine($"\nFound {urls.Count} video(s).");
            Console.WriteLine("Initializing download client...\n");
            
            // Initialize the download client
            await YDDownClient.Init(options);
            
            // Download each video
            for (int i = 0; i < urls.Count; i++)
            {
                Console.WriteLine($"\n[{i + 1}/{urls.Count}] Processing: {urls[i]}");
                await YDDownClient.DownloadAsync(urls[i]);
            }
            
            Console.WriteLine("\nAll videos processed!");
        }
    }
}