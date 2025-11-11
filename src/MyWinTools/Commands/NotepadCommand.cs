using System;
using System.Diagnostics;
using CommandLine;
using MyWinTools.Utils;

namespace MyWinTools.Commands
{
    [Verb("--notepad", HelpText = "Open a file in Notepad")]
    public class NotepadCommand
    {
        [Value(0, MetaName = "file", HelpText = "File path to open", Required = true)]
        public string FilePath { get; set; }

        public int Execute()
        {
            try
            {
                string absolutePath = PathUtils.GetAbsolutePath(FilePath);
                Process.Start("notepad.exe", absolutePath);
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