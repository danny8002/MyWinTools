using System;
using System.IO;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--copy", HelpText = "Copy file to target location with different behaviors")]
    public class CopyCommand
    {
        [Value(0, Required = true, HelpText = "Source file path")]
        public string SourcePath { get; set; }

        [Option('r', Required = false, HelpText = "Target folder path for recursive search and replace")]
        public string RecursiveTargetPath { get; set; }

        [Option('f', Required = false, HelpText = "Target file path for direct file copy")]
        public string TargetFilePath { get; set; }

        [Value(1, Required = false, HelpText = "Target folder path for simple folder copy")]
        public string TargetFolderPath { get; set; }

        public int Execute()
        {
            try
            {
                string absoluteSourcePath = PathUtils.GetAbsolutePath(SourcePath);
                
                if (!File.Exists(absoluteSourcePath))
                {
                    Console.WriteLine($"Source file not found: {absoluteSourcePath}");
                    return 1;
                }

                string sourceFileName = Path.GetFileName(absoluteSourcePath);

                // Recursive folder search and replace
                if (!string.IsNullOrEmpty(RecursiveTargetPath))
                {
                    string absoluteTargetPath = PathUtils.GetAbsolutePath(RecursiveTargetPath);
                    if (!Directory.Exists(absoluteTargetPath))
                    {
                        Console.WriteLine($"Target directory not found: {absoluteTargetPath}");
                        return 1;
                    }

                    int count = 0;
                    foreach (string file in Directory.GetFiles(absoluteTargetPath, sourceFileName, SearchOption.AllDirectories))
                    {
                        File.Copy(absoluteSourcePath, file, true);
                        count++;
                    }

                    Console.WriteLine($"Copied to {count} files recursively");
                    return 0;
                }

                // Direct file copy
                if (!string.IsNullOrEmpty(TargetFilePath))
                {
                    string absoluteTargetFilePath = PathUtils.GetAbsolutePath(TargetFilePath);
                    string targetDir = Path.GetDirectoryName(absoluteTargetFilePath);
                    
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    File.Copy(absoluteSourcePath, absoluteTargetFilePath, true);
                    Console.WriteLine($"Copied to: {absoluteTargetFilePath}");
                    return 0;
                }

                // Folder copy
                if (!string.IsNullOrEmpty(TargetFolderPath))
                {
                    string absoluteTargetFolderPath = PathUtils.GetAbsolutePath(TargetFolderPath);
                    if (!Directory.Exists(absoluteTargetFolderPath))
                    {
                        Directory.CreateDirectory(absoluteTargetFolderPath);
                    }

                    string targetFilePath = Path.Combine(absoluteTargetFolderPath, sourceFileName);
                    File.Copy(absoluteSourcePath, targetFilePath, true);
                    Console.WriteLine($"Copied to: {targetFilePath}");
                    return 0;
                }

                Console.WriteLine("No target location specified. Use -r, -f, or provide a target folder path.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}