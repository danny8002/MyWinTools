using System;
using System.IO;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--del", HelpText = "Delete a file or directory")]
    public class DeleteCommand
    {
        [Value(0, Required = true, HelpText = "Path to the file or directory to delete")]
        public string Path { get; set; }

        [Option('r', "recursive", Required = false, HelpText = "Delete directories and their contents recursively")]
        public bool Recursive { get; set; }

        public int Execute()
        {
            try
            {
                var absPath = PathUtils.GetAbsolutePath(Path);

                if (File.Exists(absPath))
                {
                    Console.WriteLine($"Deleting File: {absPath}");
                    File.Delete(absPath);
                }
                else if (Directory.Exists(absPath))
                {
                    if (!Recursive && Directory.GetFileSystemEntries(absPath).Length > 0)
                    {
                        Console.WriteLine($"Directory {absPath} is not empty. Use -r option to delete recursively.");
                        return 1;
                    }
                    Console.WriteLine($"Deleting Directory: {absPath}");
                    Directory.Delete(absPath, Recursive);
                }
                else
                {
                    Console.WriteLine($"Path not found: {absPath}");
                    return 1;
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