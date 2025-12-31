using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace web_tool_youtube_downloader
{
    /// <summary>
    /// YouTube Web Client using Playwright to extract URLs from playlists and videos
    /// </summary>
    public class YoutubeWebClient : IAsyncDisposable
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private bool _isInitialized = false;

        /// <summary>
        /// Initialize the Playwright browser
        /// </summary>
        public async Task Init(BrowserTypeLaunchOptions options)
        {
            if (_isInitialized)
            {
                Console.WriteLine("YoutubeWebClient already initialized.");
                return;
            }

            Console.WriteLine("Initializing Playwright browser...");
            Console.WriteLine("Note: Make sure Playwright browsers are installed. Run: pwsh bin/Debug/net9.0/playwright.ps1 install");

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(options);

            _isInitialized = true;
            Console.WriteLine("Browser initialized successfully.");
        }

        /// <summary>
        /// Extract URLs from YouTube input (playlist or single video)
        /// </summary>
        /// <param name="input">YouTube URL (playlist or video)</param>
        /// <returns>List of video URLs</returns>
        public async Task<List<string>> ExtractURLs(string input)
        {
            if (!_isInitialized || _browser == null)
            {
                throw new InvalidOperationException("YoutubeWebClient not initialized. Call Init() first.");
            }

            var urls = new List<string>();

            // Check if it's a YouTube playlist or video
            bool isPlaylist = input.Contains("list=") || input.Contains("/playlist");

            if (isPlaylist)
            {
                Console.WriteLine("Detected: YouTube Playlist");
                urls = await ExtractPlaylistUrlsAsync(input);
            }
            else
            {
                Console.WriteLine("Detected: YouTube Video");
                urls.Add(input);
            }

            return urls;
        }

        /// <summary>
        /// Extract all video URLs from a YouTube playlist
        /// </summary>
        private async Task<List<string>> ExtractPlaylistUrlsAsync(string playlistUrl)
        {
            if (_browser == null)
            {
                throw new InvalidOperationException("Browser not initialized.");
            }

            Console.WriteLine("Extracting playlist URLs...");

            var page = await _browser.NewPageAsync();

            try
            {
                Console.WriteLine($"Navigating to: {playlistUrl}");
                await page.GotoAsync(playlistUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

                // Wait for the content to load
                await page.WaitForSelectorAsync("#contents", new PageWaitForSelectorOptions { Timeout = 10000 });

                // Scroll to load all videos in the playlist
                Console.WriteLine("Scrolling to load all videos...");
                await ScrollToLoadAllVideos(page);

                // Execute JavaScript to extract all video URLs
                Console.WriteLine("Extracting video URLs...");
                var links = await page.EvaluateAsync<string[]>(@"
                    (function extract(){
                        var div = document.getElementById('contents');
                        var list = div.querySelectorAll('a.ytd-thumbnail');
                        var links = [];
                        for(let i=0; i<list.length; ++i)
                        {
                            links.push(list[i].href);
                        }
                        return links;
                    })();
                ");

                Console.WriteLine($"Found {links.Length} videos in the playlist");

                var result = new List<string>(links);
                await page.CloseAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting playlist URLs: {ex.Message}");
                await page.CloseAsync();
                throw;
            }
        }

        /// <summary>
        /// Scroll down the page to load all playlist items
        /// </summary>
        private static async Task ScrollToLoadAllVideos(IPage page)
        {
            // Scroll down multiple times to ensure all playlist items are loaded
            for (int i = 0; i < 5; i++)
            {
                await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                await Task.Delay(1000); // Wait for content to load
            }
        }

        /// <summary>
        /// Dispose of browser resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }

            if (_playwright != null)
            {
                _playwright.Dispose();
                _playwright = null;
            }

            _isInitialized = false;
        }
    }
}
