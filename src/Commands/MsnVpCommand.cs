using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using MyWinTools.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MyWinTools.Commands
{
    [Verb("--msn-vp", HelpText = "Process MSN validation page reports and update spec files, this command must be run in msnews-experience repo root folder")]
    public class MsnVpCommand
    {
        [Value(0, Required = true, HelpText = "Input file path containing MSN validation page links")]
        public string InputPath { get; set; }

        [Option('c', "cookie", Required = true, HelpText = "Cookie file path for HTTP requests")]
        public string CookiePath { get; set; }

        public int Execute()
        {
            try
            {
                string absoluteInputPath = PathUtils.GetAbsolutePath(InputPath);
                string absoluteCookiePath = PathUtils.GetAbsolutePath(CookiePath);

                if (!File.Exists(absoluteInputPath))
                {
                    Console.WriteLine($"Input file not found: {absoluteInputPath}");
                    return 1;
                }

                if (!File.Exists(absoluteCookiePath))
                {
                    Console.WriteLine($"Cookie file not found: {absoluteCookiePath}");
                    return 1;
                }

                string cookieContent = File.ReadAllText(absoluteCookiePath);
                ProcessLinksAsync(absoluteInputPath, cookieContent).Wait();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private async Task ProcessLinksAsync(string inputPath, string cookie)
        {
            var links = ExtractLinks(inputPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            Console.WriteLine($"Total {links.Count} reports:");
            foreach (var link in links)
            {
                Console.WriteLine(link);
            }

            using (var client = new HttpClient())
            {
                ConfigureHttpClient(client, cookie);

                foreach (var link in links)
                {
                    try
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Processing: {link}");
                        string content = await client.GetStringAsync(link);
                        var (specFile, updateFileUrl) = ExtractSpecAndUpdateInfo(content);

                        if (string.IsNullOrEmpty(specFile) || string.IsNullOrEmpty(updateFileUrl))
                        {
                            Console.WriteLine("Failed to extract spec file or update URL");
                            continue;
                        }

                        // example specFile: C:\__w\1\s\app-types\visualparity-tests\suites\weather\copilot\desktopWeatherAlert\desktop.copilot.alert.spec.json

                        // Replace the folder string before "app-types" with Environment.CurrentDirectory
                        var appTypesIndex = specFile.IndexOf("app-types", StringComparison.OrdinalIgnoreCase);
                        if (appTypesIndex >= 0)
                        {
                            var relativePath = specFile.Substring(appTypesIndex);
                            specFile = Path.Combine(Environment.CurrentDirectory, relativePath);
                        }

                        var metadataFile = specFile.Replace(".spec.json", ".metadata.json");

                        Console.WriteLine($"Spec file: {specFile}");
                        Console.WriteLine($"Metadata file: {metadataFile}");
                        Console.WriteLine($"Metadata Update file: {updateFileUrl}");

                        string updateContent = await client.GetStringAsync(updateFileUrl);
                        File.WriteAllText(metadataFile, updateContent);
                        Console.WriteLine($"Updated spec file: {metadataFile}");
                    }
                    catch (Exception ex)
                    {
                        YellowConsole($"Error processing {link}: {ex.Message}");
                    }
                }
            }
        }

        private void ConfigureHttpClient(HttpClient client, string cookie)
        {
            var headers = client.DefaultRequestHeaders;
            headers.Add("Cookie", cookie);
            headers.Add("Referer", "https://login.microsoftonline.com/");
            headers.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
            headers.Add("sec-ch-ua-arch", "\"x86\"");
            headers.Add("sec-ch-ua-bitness", "\"64\"");
            headers.Add("sec-ch-ua-full-version", "\"138.0.3351.77\"");
            headers.Add("sec-ch-ua-full-version-list", "\"Not)A;Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"138.0.7204.97\", \"Microsoft Edge\";v=\"138.0.3351.77\"");
            headers.Add("sec-ch-ua-mobile", "?0");
            headers.Add("sec-ch-ua-model", "\"\"");
            headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers.Add("sec-ch-ua-platform-version", "\"19.0.0\"");
            headers.Add("sec-fetch-dest", "document");
            headers.Add("sec-fetch-mode", "navigate");
            headers.Add("sec-fetch-site", "cross-site");
            headers.Add("sec-fetch-user", "?1");
            headers.Add("sec-ms-gec", "A1688CBFEABD5C4D9A0F3A1D118D60D7AEEC179809EA10CD5F13AB53430662FB");
            headers.Add("sec-ms-gec-version", "1-138.0.3351.77");
            headers.Add("upgrade-insecure-requests", "1");
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36 Edg/138.0.0.0");
            headers.Add("x-edge-shopping-flag", "1");
        }

        private IEnumerable<string> ExtractLinks(string inputPath)
        {
            string content = File.ReadAllText(inputPath);

            var tokens = content.Split('?', '=', '"')
                .Where(s => s.StartsWith("https://int.msn.com/msnBaselines", StringComparison.OrdinalIgnoreCase))
                .Where(s => s.EndsWith(".report.html", StringComparison.OrdinalIgnoreCase));

            return tokens;
        }

        private (string specFile, string updateFileUrl) ExtractSpecAndUpdateInfo(string content)
        {
            var tokens = content.Split('<', '>', '"');

            return (
                tokens.FirstOrDefault(s => s.EndsWith(".spec.json", StringComparison.Ordinal)),
                tokens.FirstOrDefault(s => s.EndsWith(".metadata.updated.json", StringComparison.Ordinal))
                );

            //var specRegex = new Regex(@"[^\s""']+\.spec\.json");
            //var updateRegex = new Regex(@"https://int\.msn.*?\.metadata\.updated\.json");

            //var specMatch = specRegex.Match(content);
            //var updateMatch = updateRegex.Match(content);

            //return (
            //    specMatch.Success ? specMatch.Value : null,
            //    updateMatch.Success ? updateMatch.Value : null
            //);
        }

        private void YellowConsole(string text)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }
}