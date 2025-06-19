using System;
using System.Diagnostics;
using System.IO;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--diff", HelpText = "Compare files using Beyond Compare")]
    public class BeyondCompareCommand
    {
        [Value(0, MetaName = "file1", HelpText = "First file to compare", Required = true)]
        public string File1 { get; set; }

        [Value(1, MetaName = "file2", HelpText = "Second file to compare", Required = false)]
        public string File2 { get; set; }

        [Option('u', "update", Required = false, HelpText = "Update file2 with file1's content")]
        public bool Update { get; set; }

        private const string BeyondComparePath = @"C:\Program Files\Beyond Compare 4\BCompare.exe";

        public int Execute()
        {
            try
            {
                string absolutePath1 = PathUtils.GetAbsolutePath(File1);
                string absolutePath2;

                // If File2 is not provided and File1 ends with .actual
                if (string.IsNullOrEmpty(File2) && File1.EndsWith(".actual", StringComparison.OrdinalIgnoreCase))
                {
                    string inferredPath = File1
                        .Replace(@"\bin\debug", "")
                        .Replace(@"\bin\Debug", "")
                        .Replace(".actual", "");
                    absolutePath2 = PathUtils.GetAbsolutePath(inferredPath);
                }
                else if (string.IsNullOrEmpty(File2))
                {
                    Console.WriteLine("Error: Second file path is required unless first file ends with .actual");
                    return 1;
                }
                else
                {
                    absolutePath2 = PathUtils.GetAbsolutePath(File2);
                }

                if (!File.Exists(absolutePath1))
                {
                    Console.WriteLine($"Error: File not found: {absolutePath1}");
                    return 1;
                }

                if (Update)
                {
                    try
                    {
                        File.Copy(absolutePath1, absolutePath2, true);
                        Console.WriteLine($"Updated {absolutePath2} with content from {absolutePath1}");
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating file: {ex.Message}");
                        return 1;
                    }
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = BeyondComparePath,
                    Arguments = $"\"{absolutePath1}\" \"{absolutePath2}\"",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}