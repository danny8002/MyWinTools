using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace web_tool_youtube_downloader
{
    /// <summary>
    /// YouTube Download Client using ytdown.to service
    /// </summary>
    public class YDDownClient : IAsyncDisposable
    {
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;
        private static IPage? _page;
        private static bool _isInitialized = false;
        private const string YtDownUrl = "https://ytdown.to/en2/";

        /// <summary>
        /// Initialize the download client by opening the ytdown.to page
        /// </summary>
        /// <param name="options">Browser launch options (set DownloadsPath property for custom download folder)</param>
        public static async Task Init(BrowserTypeLaunchOptions options)
        {
            if (_isInitialized)
            {
                Console.WriteLine("YDDownClient already initialized.");
                return;
            }

            Console.WriteLine("Initializing YDDownClient...");

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(options);

            _page = await _browser.NewPageAsync();
            await _page.GotoAsync(YtDownUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            _isInitialized = true;
            Console.WriteLine($"YDDownClient initialized. Page loaded: {YtDownUrl}");
        }

        /// <summary>
        /// Downloads a YouTube video from the given URL
        /// </summary>
        /// <param name="videoUrl">The YouTube video URL to download</param>
        public static async Task DownloadAsync(string videoUrl)
        {
            if (!_isInitialized || _page == null)
            {
                throw new InvalidOperationException("YoutubeWebClient not initialized. Call Init() first.");
            }

            if (_page == null)
            {
                throw new InvalidOperationException("Failed to initialize YDDownClient page.");
            }

            Console.WriteLine($"[YDDownClient] Processing: {videoUrl}");

            try
            {
                // Navigate to the ytdown.to page if not already there
                if (!_page.Url.StartsWith(YtDownUrl))
                {
                    await _page.GotoAsync(YtDownUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                }

                // Execute JavaScript to fill in the URL and click the first button
                await _page.EvaluateAsync($@"
                    (function inspect_youtube(){{
                        var box = document.getElementById('postUrl');
                        box.value = '{videoUrl}';
                        
                        var btn = document.querySelector('.input-group .btn-download');
                        btn.click();
                    }})();
                ");

                Console.WriteLine("Submitted URL for processing...");

                // Wait 3 seconds for the download link to appear
                await Task.Delay(3000);

                // Execute JavaScript to click the download button
                await _page.EvaluateAsync(@"
                    (function download(){
                        var btn = document.querySelector('.box .btn-download');
                        btn.click();
                    })();
                ");

                Console.WriteLine($"Download initiated for: {videoUrl}");

                // Wait a bit for download to start
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YDDownClient] Error downloading {videoUrl}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public static async Task Cleanup()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }

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
            Console.WriteLine("YDDownClient cleaned up.");
        }

        public async ValueTask DisposeAsync()
        {
            await Cleanup();
        }
    }
}
