using System;
using System.Diagnostics;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--notepad2", HelpText = "Open a file in Notepad++")]
    public class Notepad2Command
    {
        [Value(0, MetaName = "file", HelpText = "File path to open", Required = true)]
        public string FilePath { get; set; }

        public int Execute()
        {
            try
            {
                string absolutePath = PathUtils.GetAbsolutePath(FilePath);
                Process.Start("C:/Program Files/Notepad++/notepad++.exe", absolutePath);
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