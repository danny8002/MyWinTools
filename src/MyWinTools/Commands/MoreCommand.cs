using System;
using System.IO;
using System.Linq;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--more", HelpText = "Display content of a text file")]
    public class MoreCommand
    {
        [Value(0, Required = true, HelpText = "Path to the file to display")]
        public string Path { get; set; }

        [Option('n', "lines", Required = false, Default = 0, HelpText = "Number of lines to display at once (0 for all)")]
        public int LinesPerPage { get; set; }

        public int Execute()
        {
            try
            {
                var absPath = PathUtils.GetAbsolutePath(Path);

                if (!File.Exists(absPath))
                {
                    Console.WriteLine($"File not found: {absPath}");
                    return 1;
                }

                var lines = File.ReadAllLines(absPath);

                if (LinesPerPage <= 0)
                {
                    // Display all content at once
                    Console.Write(File.ReadAllText(absPath));
                    return 0;
                }

                // Display content page by page
                for (int i = 0; i < lines.Length; i += LinesPerPage)
                {
                    var pageLines = lines.Skip(i).Take(LinesPerPage);
                    foreach (var line in pageLines)
                    {
                        Console.WriteLine(line);
                    }

                    if (i + LinesPerPage < lines.Length)
                    {
                        Console.WriteLine("Press any key for more...");
                        Console.ReadKey(true);
                    }
                }

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